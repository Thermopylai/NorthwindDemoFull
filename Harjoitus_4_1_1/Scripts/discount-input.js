$(function () {
    function tryParsePercentageLikeBinder(raw) {
        if (raw == null) return null;

        var s = String(raw).trim();
        if (s === '') return null;

        // Remove % and whitespace (incl. NBSP), like the binder
        s = s.replace(/%/g, '');
        s = s.replace(/\s|\u00A0/g, '');

        // Keep only digits and separators, but remove sign)
        s = s.replace(/[^\d.,]/g, '');
        if (s === '') return NaN;

        var lastComma = s.lastIndexOf(',');
        var lastDot = s.lastIndexOf('.');
        var decPos = Math.max(lastComma, lastDot);

        if (lastComma >= 0 && lastDot >= 0) {
            // Both exist: last one is decimal separator, other is grouping
            if (lastComma > lastDot) {
                // "1.234,56" => remove '.' grouping, comma->dot decimal
                s = s.replace(/\./g, '');
                s = replaceLast(s, ',', '.');
            } else {
                // "1,234.56" => remove ',' grouping, dot is decimal
                s = s.replace(/,/g, '');
                // dot stays as decimal
            }
        } else if (lastComma >= 0) {
            // Only comma: treat as decimal separator
            s = replaceLast(s, ',', '.');
        } else {
            // Only dot or no separator: OK as-is
        }

        // Strict numeric check after normalization
        if (!/^\d+(\.\d+)?$/.test(s)) return NaN;

        var n = Number(s);
        return isFinite(n) ? n : NaN;

        function replaceLast(input, searchChar, replaceChar) {
            var pos = input.lastIndexOf(searchChar);
            if (pos < 0) return input;
            return input.substring(0, pos) + replaceChar + input.substring(pos + 1);
        }
    }

    $('.discount-input').on('blur', function () {
        var raw = $(this).val();
        var n = tryParsePercentageLikeBinder(raw);

        // If user typed something non-empty and it's not parseable -> mark red
        if ((raw != null && String(raw).trim() !== '' && isNaN(n)) || (n > 100)) {
            $(this).addClass('input-validation-error');
            return;
        }

        $(this).removeClass('input-validation-error');

        // Leave empty alone; [Required] is server-side
        if (raw == null || String(raw).trim() === '') return;

        // Format for display (fi-FI)
        $(this).val(n.toLocaleString('fi-FI', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }));
    });

    // Cosmetic: clear red border as user edits
    $('.discount-input').on('input', function () {
        $(this).removeClass('input-validation-error');
    });
});