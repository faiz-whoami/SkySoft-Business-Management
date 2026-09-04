(function ($) {
    var partyLabels = {
        Sale: "Customer",
        Purchase: "Supplier",
        Service: "Vendor"
    };

    function readCatalog() {
        var raw = $("#txn-catalog").text();
        if (!raw) {
            return { Types: [], Parties: {}, Products: [] };
        }
        return JSON.parse(raw);
    }

    function escapeHtml(value) {
        return $("<div>").text(value == null ? "" : String(value)).html();
    }

    function formatMoney(value) {
        var amount = Number(value);
        if (isNaN(amount)) {
            amount = 0;
        }
        return "$" + amount.toFixed(2);
    }

    function productOptions(products) {
        var html = '<option value="">Custom line</option>';
        for (var i = 0; i < products.length; i++) {
            var product = products[i];
            var label = (product.Sku ? product.Sku + " · " : "") + product.ProductName;
            html += '<option value="' + product.ProductId + '" data-price="' + product.UnitPrice + '" data-name="' + escapeHtml(product.ProductName) + '">'
                + escapeHtml(label) + "</option>";
        }
        return html;
    }

    function fillTypes($select, types, selected) {
        $select.empty();
        for (var i = 0; i < types.length; i++) {
            $select.append($("<option>", { value: types[i], text: types[i] }));
        }
        if (selected) {
            $select.val(selected);
        }
    }

    function fillParties($select, parties, selected) {
        $select.empty();
        $select.append($("<option>", { value: "", text: "Select…" }));
        for (var i = 0; i < parties.length; i++) {
            $select.append($("<option>", { value: parties[i].Id, text: parties[i].Name }));
        }
        if (selected) {
            $select.val(String(selected));
        }
    }

    function lineTotal($row) {
        var qty = parseInt($row.find(".txn-qty").val(), 10) || 0;
        var price = parseFloat($row.find(".txn-price").val()) || 0;
        return qty * price;
    }

    function refreshTotals() {
        var total = 0;
        $("#txn-lines tr.txn-line").each(function () {
            var amount = lineTotal($(this));
            total += amount;
            $(this).find(".txn-line-total").text(formatMoney(amount));
        });
        $("#txn-grand-total").text(formatMoney(total));
    }

    function addLine(products, line) {
        var $row = $('<tr class="txn-line">'
            + '<td><select class="form-select txn-product">' + productOptions(products) + "</select></td>"
            + '<td><input class="form-control txn-desc" maxlength="200" /></td>'
            + '<td><input class="form-control txn-qty text-end" type="number" min="1" step="1" value="1" /></td>'
            + '<td><input class="form-control txn-price text-end" inputmode="decimal" value="0.00" /></td>'
            + '<td class="text-end fw-semibold txn-line-total">$0.00</td>'
            + '<td><button type="button" class="btn btn-sm btn-outline-danger txn-remove">Remove</button></td>'
            + "</tr>");

        if (line) {
            if (line.ProductId) {
                $row.find(".txn-product").val(String(line.ProductId));
            }
            $row.find(".txn-desc").val(line.Description || "");
            $row.find(".txn-qty").val(line.Quantity || 1);
            $row.find(".txn-price").val(Number(line.UnitPrice || 0).toFixed(2));
        }

        $("#txn-lines").append($row);
        refreshTotals();
    }

    $(function () {
        var data = readCatalog();
        var $type = $("#TransactionTypeName");
        var $party = $("#PartyId");
        var selectedType = $type.data("selected") || (data.Types[0] || "Sale");
        var selectedParty = $party.data("selected");

        fillTypes($type, data.Types, selectedType);

        function applyType() {
            var typeName = $type.val();
            $("#party-label").text(partyLabels[typeName] || "Party");
            fillParties($party, data.Parties[typeName] || [], selectedParty);
            selectedParty = null;
        }

        applyType();
        $type.on("change", applyType);

        var existing = [];
        try {
            existing = JSON.parse($("#ItemsJson").val() || "[]");
        } catch (e) {
            existing = [];
        }

        if (existing && existing.length) {
            for (var i = 0; i < existing.length; i++) {
                addLine(data.Products, existing[i]);
            }
        } else {
            addLine(data.Products, null);
        }

        $("#txn-add-line").on("click", function () {
            addLine(data.Products, null);
        });

        $("#txn-lines").on("click", ".txn-remove", function () {
            if ($("#txn-lines tr.txn-line").length === 1) {
                return;
            }
            $(this).closest("tr").remove();
            refreshTotals();
        });

        $("#txn-lines").on("change", ".txn-product", function () {
            var $option = $(this).find("option:selected");
            var $row = $(this).closest("tr");
            var name = $option.data("name");
            var price = $option.data("price");
            if (name) {
                $row.find(".txn-desc").val(name);
            }
            if (price !== undefined && price !== "") {
                $row.find(".txn-price").val(Number(price).toFixed(2));
            }
            refreshTotals();
        });

        $("#txn-lines").on("input change", ".txn-qty, .txn-price", refreshTotals);

        $("#txn-form").on("submit", function (e) {
            var lines = [];
            var error = "";
            $("#txn-lines tr.txn-line").each(function () {
                var $row = $(this);
                var productVal = $row.find(".txn-product").val();
                var description = $.trim($row.find(".txn-desc").val());
                var quantity = parseInt($row.find(".txn-qty").val(), 10) || 0;
                var unitPrice = parseFloat($row.find(".txn-price").val());
                if (isNaN(unitPrice)) {
                    unitPrice = 0;
                }
                if (!description) {
                    error = "Each line needs a description.";
                    return;
                }
                if (quantity <= 0) {
                    error = "Quantity must be greater than zero.";
                    return;
                }
                if (unitPrice < 0) {
                    error = "Unit price cannot be negative.";
                    return;
                }
                lines.push({
                    ProductId: productVal ? parseInt(productVal, 10) : null,
                    Description: description,
                    Quantity: quantity,
                    UnitPrice: unitPrice
                });
            });

            var $error = $("#txn-lines-error");
            if (error || lines.length === 0) {
                e.preventDefault();
                $error.text(error || "Add at least one line item.").prop("hidden", false);
                return;
            }

            if (!$party.val()) {
                e.preventDefault();
                $error.text("Select a party for this transaction.").prop("hidden", false);
                return;
            }

            $error.prop("hidden", true).text("");
            $("#ItemsJson").val(JSON.stringify(lines));
        });
    });
})(jQuery);
