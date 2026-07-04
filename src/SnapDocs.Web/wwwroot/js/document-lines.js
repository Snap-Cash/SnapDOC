let itemIndex = 1;

function productOptionsHtml(){
    const products = window.productCatalog || [];
    const options = products.map(p => `<option value="${p.id}">${escapeHtml(p.name)}</option>`).join('');
    return `<option value="">اختيار منتج</option>${options}`;
}

function escapeHtml(value){
    return String(value || '').replace(/[&<>'"]/g, function(c){
        return {'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c];
    });
}

function addItem(){
    const tbody = document.querySelector('#itemsTable tbody');
    const row = document.createElement('tr');
    row.innerHTML = `
        <td><select name="Items[${itemIndex}].ProductId" class="product-select" onchange="applyProduct(this)">${productOptionsHtml()}</select></td>
        <td><input name="Items[${itemIndex}].Description" placeholder="وصف البند" /></td>
        <td><input name="Items[${itemIndex}].Quantity" type="number" step="0.001" value="1" oninput="recalc()" /></td>
        <td><input name="Items[${itemIndex}].UnitPrice" type="number" step="0.01" value="0" oninput="recalc()" /></td>
        <td><input name="Items[${itemIndex}].Discount" type="number" step="0.01" value="0" oninput="recalc()" /></td>
        <td class="line-total">0.00</td>
        <td><button type="button" class="btn small danger" onclick="removeRow(this)">حذف</button></td>`;
    tbody.appendChild(row);
    itemIndex++;
    recalc();
}

function applyProduct(select){
    const id = select.value;
    if(!id || !window.productCatalog) return;
    const product = window.productCatalog.find(p => String(p.id).toLowerCase() === String(id).toLowerCase());
    if(!product) return;
    const row = select.closest('tr');
    const inputs = row.querySelectorAll('input');
    inputs[0].value = product.description || product.name || '';
    inputs[2].value = Number(product.salePrice || 0).toFixed(2);
    const taxInput = document.querySelector('[name="TaxRate"]');
    if(taxInput && (!taxInput.value || Number(taxInput.value) === 0) && product.taxRate){
        taxInput.value = product.taxRate;
    }
    recalc();
}

function removeRow(btn){
    const tbody = document.querySelector('#itemsTable tbody');
    if(tbody && tbody.rows.length === 1){
        const row = btn.closest('tr');
        row.querySelectorAll('input').forEach((input, idx) => input.value = idx === 1 ? '1' : '0');
        const desc = row.querySelector('input[name$=".Description"]');
        if(desc) desc.value = '';
        const select = row.querySelector('select');
        if(select) select.value = '';
        recalc();
        return;
    }
    btn.closest('tr').remove();
    recalc();
}

function n(v){return parseFloat(v||'0')||0;}
function money(v){return v.toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2});}

function recalc(){
    let subtotal = 0;
    document.querySelectorAll('#itemsTable tbody tr').forEach(row => {
        const inputs = row.querySelectorAll('input');
        const qty = n(inputs[1]?.value);
        const price = n(inputs[2]?.value);
        const disc = n(inputs[3]?.value);
        const line = Math.max(0, qty * price - disc);
        const lineCell = row.querySelector('.line-total');
        if(lineCell) lineCell.innerText = money(line);
        subtotal += line;
    });

    const discount = Math.min(n(document.querySelector('[name="Discount"]')?.value), subtotal);
    const taxRate = n(document.querySelector('[name="TaxRate"]')?.value);
    const tax = (subtotal - discount) * taxRate / 100;
    const total = subtotal - discount + tax;

    setText('subTotal', money(subtotal));
    setText('docDiscount', money(discount));
    setText('taxValue', money(tax));
    setText('grandTotal', money(total));
}

function setText(id, value){
    const el = document.getElementById(id);
    if(el) el.innerText = value;
}

recalc();
