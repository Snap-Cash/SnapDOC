window.QuotationStudio = (function(){
    function openTab(name){
        document.querySelectorAll('[data-quote-tab]').forEach(x => x.classList.toggle('active', x.dataset.quoteTab === name));
        document.querySelectorAll('[data-quote-section]').forEach(x => x.classList.toggle('active', x.dataset.quoteSection === name));
    }

    function updateTotals(){
        if(typeof recalc === 'function') recalc();
        const grand = document.getElementById('grandTotal')?.innerText || '0.00';
        const floating = document.getElementById('floatingQuoteTotal');
        if(floating) floating.innerText = grand;
    }

    function focusItems(){
        openTab('items');
        setTimeout(() => document.querySelector('#itemsTable input, #itemsTable select')?.focus(), 80);
    }

    document.addEventListener('keydown', function(e){
        if(e.key === 'F2'){
            e.preventDefault();
            document.getElementById('docForm')?.requestSubmit();
        }
        if((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's'){
            e.preventDefault();
            document.getElementById('docForm')?.requestSubmit();
        }
    });

    document.addEventListener('input', function(){ updateTotals(); });
    document.addEventListener('DOMContentLoaded', updateTotals);

    return { openTab, updateTotals, focusItems };
})();
