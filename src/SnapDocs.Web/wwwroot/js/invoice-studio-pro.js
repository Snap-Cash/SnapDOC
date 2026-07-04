
(function(){
    function money(v){ return (Number(v)||0).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2}); }
    window.InvoiceStudioPro = {
        openTab: function(name){
            document.querySelectorAll('[data-invoice-tab]').forEach(x=>x.classList.toggle('active', x.dataset.invoiceTab===name));
            document.querySelectorAll('[data-invoice-section]').forEach(x=>x.classList.toggle('active', x.dataset.invoiceSection===name));
        },
        focusItems: function(){ this.openTab('items'); setTimeout(()=>document.querySelector('#itemsTable input[name$=".Description"]')?.focus(),120); },
        updateSideTotals: function(){
            if(typeof recalc === 'function') recalc();
            const grand = document.getElementById('grandTotal')?.innerText || '0.00';
            const floating = document.getElementById('floatingInvoiceTotal');
            if(floating) floating.innerText = grand;
        },
        bindShortcuts: function(){
            document.addEventListener('keydown', function(e){
                if((e.ctrlKey || e.metaKey) && e.key.toLowerCase()==='s'){
                    const form = document.getElementById('docForm');
                    if(form){ e.preventDefault(); form.requestSubmit(); }
                }
                if((e.ctrlKey || e.metaKey) && e.key.toLowerCase()==='p'){
                    const printLink = document.querySelector('[data-invoice-print]');
                    if(printLink){ e.preventDefault(); window.open(printLink.href, '_blank'); }
                }
                if(e.key==='F2'){
                    const form = document.getElementById('docForm');
                    if(form){ e.preventDefault(); form.requestSubmit(); }
                }
            });
        }
    };
    document.addEventListener('input', function(e){ if(e.target.closest('.invoice-pro-shell')) InvoiceStudioPro.updateSideTotals(); });
    document.addEventListener('DOMContentLoaded', function(){ InvoiceStudioPro.bindShortcuts(); InvoiceStudioPro.updateSideTotals(); });
})();
