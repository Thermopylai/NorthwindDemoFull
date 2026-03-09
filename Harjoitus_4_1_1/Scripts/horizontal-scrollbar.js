document.addEventListener("DOMContentLoaded", function () {
    // Etsitään kaikki taulukoiden kääreet
    const wrappers = document.querySelectorAll('.table-wrapper');

    wrappers.forEach((actualWrapper, index) => {
        const table = actualWrapper.querySelector('table');

        // Luodaan dynaamisesti yläpalkki
        const topWrapper = document.createElement('div');
        topWrapper.className = 'top-scroll-wrapper';
        topWrapper.innerHTML = `<div style="width:${table.offsetWidth}px; height:1px;"></div>`;

        // Lisätään se ennen taulukkoa
        actualWrapper.parentNode.insertBefore(topWrapper, actualWrapper);

        // Synkronoidaan vieritys molempiin suuntiin
        topWrapper.onscroll = () => actualWrapper.scrollLeft = topWrapper.scrollLeft;
        actualWrapper.onscroll = () => topWrapper.scrollLeft = actualWrapper.scrollLeft;

        // Päivitetään leveys, jos ikkunaa säädetään tai haku suodattaa rivejä
        const updateWidth = () => {
            topWrapper.firstChild.style.width = table.scrollWidth + "px";
        };
        window.addEventListener('resize', updateWidth);
        // Voit kutsua updateWidth() myös hakufunktion lopussa
    });
});
