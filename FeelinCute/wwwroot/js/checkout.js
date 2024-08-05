function removeProductForCheckOut(productid) {
    removeProduct(productid).then(function (sum) {
        document.getElementById("total-sum").innerHTML = sum;
        var product = document.getElementById(`prd-${productid}`);
        product.remove();
    })
        .catch(function (error) {
            console.error('Error:', error);
        });
}
function changeQuantityForCheckOut(productid, buttonmode) {
    changeQuantity(productid, buttonmode).then(function (sum) {
        document.getElementById("total-sum").innerHTML = sum;
    })
        .catch(function (error) {
            console.error('Error:', error);
        });
    var quantityElement = document.getElementById("checkoutquantity-" + productid);
    if (buttonmode == "<") {
        var currentQuantity = parseInt(quantityElement.innerText);
        if (currentQuantity > 1) {

            quantityElement.innerText = currentQuantity - 1;
            updatePrice(productid, currentQuantity, -1, "sum-");
        }
    }
    else if (buttonmode == ">") {
        var currentQuantity = parseInt(quantityElement.innerText);
        quantityElement.innerText = currentQuantity + 1;
        updatePrice(productid, currentQuantity, 1, "sum-");
    }
}
