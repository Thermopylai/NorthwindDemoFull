function filterTable(inputId, tableId) {
    const input = document.getElementById(inputId);
    const filter = input.value.toUpperCase();
    const table = document.getElementById(tableId);
    const rows = table.querySelectorAll("tbody tr:not([id^='no-results'])");
    const noResultsRow = document.getElementById("no-results-" + tableId.replace("-table", ""));

    let visibleCount = 0;

    for (let i = 0; i < rows.length; i++) {
        let matchFound = false;
        const cells = rows[i].getElementsByTagName("td");

        for (let j = 0; j < cells.length; j++) {
            const cellText = cells[j].textContent || cells[j].innerText;
            if (cellText.toUpperCase().indexOf(filter) > -1) {
                matchFound = true;
                break;
            }
        }

        if (matchFound) {
            rows[i].style.display = "";
            visibleCount++;
        } else {
            rows[i].style.display = "none";
        }
    }

    // Näytetään ilmoitus jos yhtään riviä ei löytynyt
    if (noResultsRow) {
        noResultsRow.style.display = (visibleCount === 0) ? "block" : "none";
    }
}
