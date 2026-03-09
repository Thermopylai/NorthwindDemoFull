(() => {
    'use strict'

    const initSorter = () => {

        const getCellValue = (tr, idx) =>
            (tr.children[idx] && (tr.children[idx].textContent || tr.children[idx].innerText)) || '';

        const tryParseFiDate = (value) => {
            if (value == null) return null;

            const s = value.toString().trim();
            if (s === '') return null;

            // dd.MM.yyyy
            const m = /^(\d{1,2})\.(\d{1,2})\.(\d{4})$/.exec(s);
            if (!m) return null;

            const day = parseInt(m[1], 10);
            const month = parseInt(m[2], 10) - 1;
            const year = parseInt(m[3], 10);

            const d = new Date(year, month, day);

            // Validate because Date() normalizes invalid inputs (e.g., 32.01.2026 -> Feb 1)
            if (d.getFullYear() !== year || d.getMonth() !== month || d.getDate() !== day) {
                return null;
            }

            return d;
        };

        const comparer = (idx, asc) => (a, b) => ((v1, v2) => {
            v1 = (v1 ?? '').toString().trim().replace(/\s|\u00A0|#/g, '');
            v2 = (v2 ?? '').toString().trim().replace(/\s|\u00A0|#/g, '');

            // 1) Date comparison first (fi-FI dd.MM.yyyy)
            const d1 = tryParseFiDate(v1);
            const d2 = tryParseFiDate(v2);
            if (d1 && d2) {
                return d1.getTime() - d2.getTime();
            }

            // 2) Numeric comparison
            const n1 = parseFloat(v1);
            const n2 = parseFloat(v2);
            const isNum1 = v1 !== '' && !isNaN(n1) && isFinite(n1);
            const isNum2 = v2 !== '' && !isNaN(n2) && isFinite(n2);

            return isNum1 && isNum2
                ? (n1 - n2)
                : v1.toString().localeCompare(v2.toString());
        })(getCellValue(asc ? a : b, idx), getCellValue(asc ? b : a, idx));

        // Initialize each table independently
        document.querySelectorAll('table').forEach(table => {
            const firstSortableTh = table.querySelector('th.sortable');
            if (firstSortableTh && !firstSortableTh.dataset.sort) {
                firstSortableTh.dataset.sort = 'asc';
            }

            table.querySelectorAll('th.sortable').forEach(th => {
                th.addEventListener('click', () => {
                    const tbody = table.querySelector('tbody');
                    if (!tbody) return;

                    // Compute column index within THIS table’s header row
                    const headerRow = th.closest('tr');
                    const headers = Array.from(headerRow.children);
                    const colIndex = headers.indexOf(th);
                    if (colIndex < 0) return;

                    const isFirstColumn = colIndex === 0;
                    const alreadyHandledInitial = table.dataset.initialSortHandled === 'true';

                    // Normal toggle behavior based on this TH state
                    let asc = th.dataset.sort !== 'asc';

                    // Special case: first click on first column goes DESC (per-table)
                    if (isFirstColumn && !alreadyHandledInitial && asc) {
                        asc = false;
                        table.dataset.initialSortHandled = 'true';
                    }

                    // Hide sort icons only in THIS table
                    table.querySelectorAll('th.sortable').forEach(element => {
                        const icon = element.querySelector('i');
                        if (icon) icon.classList.add('d-none');
                    });

                    // Show this header's icon + correct direction
                    const icon = th.querySelector('i');
                    if (icon) {
                        icon.classList.remove('d-none');
                        icon.setAttribute('class', asc ? 'bi-arrow-down me-1' : 'bi-arrow-up me-1');
                    }

                    // Clear only THIS table's headers sort state, set this one
                    headerRow.querySelectorAll('th').forEach(h => delete h.dataset.sort);
                    th.dataset.sort = asc ? 'asc' : 'desc';

                    // Sort only THIS table's rows
                    Array.from(tbody.querySelectorAll('tr'))
                        .sort(comparer(colIndex, asc))
                        .forEach(tr => tbody.appendChild(tr));
                });
            });
        });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSorter);
    } else {
        initSorter();
    }
})();