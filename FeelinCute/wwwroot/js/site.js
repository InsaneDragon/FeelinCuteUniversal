document.addEventListener("DOMContentLoaded", function () {
    var cartPopup = document.getElementById("cart-popup");
    var openSearchBtn = document.getElementById("search-btn");
    var cartBtn = document.getElementById("cart-btn");
    var closeCart = document.getElementById("cart-close");
    var closeSearch = document.getElementById("search-close");
    var likedPopup = document.getElementById("liked-popup");
    var searchPopup = document.getElementById("search-popup");
    var likedBtn = document.getElementById("liked-btn");
    var closeLiked = document.getElementById("likedCloseBtn");
    var addtocartButtons = document.querySelectorAll('.AddToCart');
    var dropbtns = document.querySelectorAll('.dropbtn');
    var prevScrollpos = window.pageYOffset
    const textContainer = document.querySelector('.text-container');
    const textWrapper = document.querySelector('.text-wrapper');
    const textItems = document.querySelectorAll('.text-item');
    document.querySelectorAll('.product-img').forEach(img => {
        img.addEventListener('mouseover', () => {
            const hoverImage = img.getAttribute('data-hover-image');
            if (hoverImage) {
                img.style.backgroundImage = `url(${hoverImage})`;
                img.classList.add('product-img-expand');
            }
        });
        // On mouse out, revert to the original image
        img.addEventListener('mouseout', () => {
            const originalImage = img.getAttribute('original-image');
            img.style.backgroundImage = `url(${originalImage})`;
            img.classList.remove('product-img-expand');
        });
    })
    $(".like-checkbox").change(function () {
        var checkboxId = $(this).attr('id'); // Get the ID of the checked checkbox
        var buttonid = checkboxId.split('-')[1]; // Extract the product ID from the checkbox ID
        if (this.checked) {
            FlyingAnimation(buttonid,"liked-btn");
            $.ajax({
                url: '/Cart/AddLikedItem',
                type: 'POST',
                data: { productid: buttonid },
                success: function (response) {
                    var likedcount = document.querySelectorAll('.like-count');
                    likedcount.forEach(count => count.innerText = response.length);
                    document.getElementById('liked-products').innerHTML = '';
                    response.forEach((product) => {
                        addProductToLiked(product);
                    });
                },
                error: function (error) {
                    console.error('Error:', error);
                }
            });
        }
        else {
            $.ajax({
                url: '/Cart/RemoveLikedItem',
                type: 'DELETE',
                data: { productid: buttonid },
                success: function (response) {
                    var cartcount = document.querySelector('.like-count');
                    cartcount.innerText = parseInt(cartcount.innerText) - 1;
                    var productElement = document.getElementById("liked-" + buttonid);
                    productElement.remove();
                },
                error: function (error) {
                    console.error('Error:', error);
                }
            });
        }
    });
    dropbtns.forEach(function (button) {
        button.addEventListener('click', function () {
            button.parentNode.classList.toggle('open');
        })
    })
    function FlyingAnimation(id,flytoid) {
        document.getElementById("navbar").style.top = "47px";
        const miniImage = document.getElementById(`mini-image-${id}`);
        const miniImageRect = miniImage.getBoundingClientRect();
        const cartBtn = document.getElementById(flytoid); // Assuming "cart-btn" is the ID of the cart button
        const cartRect = cartBtn.getBoundingClientRect();
        const deltaX = cartRect.left - miniImageRect.left;
        const deltaY = cartRect.top - miniImageRect.top;
        const clonedImage = miniImage.cloneNode();
        $(`#product-container-${id} .product-wrapper`).append(clonedImage);
        clonedImage.style.visibility = "visible";
        setTimeout(function () {
            clonedImage.style.transform = `translate3d(${deltaX}px, ${deltaY}px, 0)`;
            setTimeout(function () {
                clonedImage.parentNode.removeChild(clonedImage);
                cartBtn.classList.add('shake');
            }, 1000);
            cartBtn.classList.remove('shake');
        }, 0);
    }
    window.onscroll = function () {
        var targetElement = document.body;
        var distanceFromTop = targetElement.getBoundingClientRect().top;
        var currentScrollPos = window.pageYOffset;
        if (prevScrollpos > currentScrollPos) {
            document.getElementById("navbar").style.top = "47px";
        } else if (distanceFromTop < 0) {
            document.getElementById("navbar").style.top = "-92px";
        }
        prevScrollpos = currentScrollPos;
    }

    cartBtn.addEventListener("click", function () {
        var width = document.body.offsetWidth;
        if (width < 1250) {
            cartPopup.style.width = "100%";
        }
        else {
            cartPopup.style.width = "35%";
        }
    });
    openSearchBtn.addEventListener("click", function () {
            cartPopup.style.width = "100%";
    });
    closeCart.addEventListener("click", function () {
        cartPopup.style.width = "0";
    });
    closeSearch.addEventListener("click", function () {
        searchPopup.style.width = "0";
    });
    likedBtn.addEventListener("click", function () {
        var width = document.body.offsetWidth;
        if (width < 1250) {
            likedPopup.style.width = "100%";
        }
        else {
            likedPopup.style.width = "35%";
        }
    });
    closeLiked.addEventListener("click", function () {
        likedPopup.style.width = "0";
    });
    addtocartButtons.forEach(function (button) {

        button.addEventListener('click', function (event) {
            let buttonid = event.target.id;
            FlyingAnimation(buttonid,"cart-btn");
            let quantityElement = document.getElementById(`prd-quantity-${buttonid}`);
            let quantity = quantityElement ? quantityElement.innerText : null;
            $.ajax({
                url: '/Cart/AddToCart',
                type: 'POST',
                data: { productid: buttonid, amount: quantity },
                success: function (response) {
                    var cartcount = document.querySelectorAll('.cart-count');
                    cartcount.forEach((count) => count.innerText = response.length);
                    document.getElementById('cart-products').innerHTML = '';
                    response.forEach((product) => {
                        addProductToCart(product);
                    });
                },
                error: function (error) {
                    console.error('Error:', error);
                    // Handle errors here
                }
            });
        });
    });
    function addProductToCart(product) {
        let discountedPrice = product.discount !== null ? product.price - (product.discount / 100 * product.price) : product.price;
        let sum = discountedPrice * product.pCount;
        var productDiv = document.createElement('div');
        productDiv.id = 'product-' + product.id;
        productDiv.className = 'product-info';
        productDiv.innerHTML = `
            <div class="image-container">
            <a href="/Home/ProductDetails?productId=${product.id}" style="background-image: url('/images/${product.image}');" class="cart-img"></a>
                </div>
                <div class="line">
                <span class="prd-name">${product.name}</span>
                <span>$</span>
                <span id="price-${product.id}" class="prd-price">${sum}</span>
                </div>
                <div class="line">
                    <button onclick="changeQuantity(${product.id},'<')" class="arrow">&lt;</button>
                    <span id="quantity-${product.id}">${product.pCount}</span>
                    <button onclick="changeQuantity(${product.id},'>')" class="arrow">&gt;</button>
                </div>
                <div class="action-links">
                <a href="/Home/ProductDetails?productId=${product.id}" class="btn-outline-info form-control">Edit</a>
                    <button onclick="removeProduct(${product.id})" class="btn-outline-danger form-control">Remove</button>
                </div>
        `;
        var cartContainer = document.getElementById('cart-products');
        cartContainer.appendChild(productDiv);
    }
    function addProductToLiked(product) {
        var productDiv = document.createElement('div');
        productDiv.id = 'liked-' + product.id;
        productDiv.className = 'liked-info';
        productDiv.innerHTML = `
             <a>
                    <img src="/images/${product.image}" />
                    <span class="prd-name">${product.name}</span>
                </a>
        `;
        var likedContainer = document.getElementById('liked-products');
        likedContainer.appendChild(productDiv);
    }

});
async function GetProductsSum() {
    try {
        const response = await $.ajax({
            url: '/Cart/GetProductsSum',
            type: 'GET'
        });
        return response;
    } catch (error) {
        console.error('Error:', error);
        return 0; // or any default value indicating error
    }
}
function decreaseQuantity(productId) {
    var quantityElement = document.getElementById("quantity-" + productId);
    var currentQuantity = parseInt(quantityElement.innerText);
    if (currentQuantity > 1) {

        quantityElement.innerText = currentQuantity - 1;
        updatePrice(productId, currentQuantity, -1, "price-");
    }
}

function increaseQuantity(productId) {
    var quantityElement = document.getElementById("quantity-" + productId);
    var currentQuantity = parseInt(quantityElement.innerText);
    quantityElement.innerText = currentQuantity + 1;
    updatePrice(productId, currentQuantity, 1, "price-");
}

function updatePrice(productId, count, operation, elementid) {
    var priceText = document.getElementById(elementid + productId);
    var currentPrice = parseInt(priceText.innerText);
    var priceperone = currentPrice / count;
    priceText.innerText = currentPrice + (priceperone * operation);
}
function removeProduct(productId) {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: '/Cart/RemoveCartProduct',
            type: 'DELETE',
            data: {
                productid: productId,
            },
            success: function (response) {
                var cartcount = document.querySelectorAll('.cart-count');
                cartcount.forEach((count) => count.innerText = parseInt(count.innerText) - 1);
                var productElement = document.getElementById("product-" + productId);
                productElement.remove();
                GetProductsSum().then(function (sum) {
                    resolve(sum);
                }).catch(function (error) {
                    reject(error);
                });
            },
            error: function (error) {
                console.error('Error:', error);
                reject(error);
            }
        });
    });
}
function changeQuantity(buttonid, buttonmode) {
    return new Promise((resolve, reject) => {
        var quantity = parseInt(document.getElementById("quantity-" + buttonid).innerText);
        if (buttonmode == '<' && quantity <= 1) { }
        else {
            $.ajax({
                url: '/Cart/ChangeProductAmount',
                type: 'PUT',
                data: {
                    productid: buttonid,
                    operation: buttonmode
                },
                success: function (response) {
                    if (buttonmode == "<") {
                        decreaseQuantity(buttonid);
                    }
                    else if (buttonmode == ">") {
                        increaseQuantity(buttonid);
                    }
                    GetProductsSum().then(function (sum) {
                        resolve(sum);
                    }).catch(function (error) {
                        reject(error);
                    });
                },
                error: function (error) {
                    console.error('Error:', error);
                    reject(error);
                }
            });

        }
    });
}
