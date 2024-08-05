document.addEventListener("DOMContentLoaded", function () {
    var smallImages = document.querySelectorAll('.small-image');
    var mainImage = document.getElementById('image');
    var changeAmountBtns = document.querySelectorAll('.ChangeAmount');
    $(".like-checkbox").change(function () {
        if (this.checked) {
            $('.like-text').text("Liked!");
        }
        else {
            $('.like-text').text("Like");
        }
    });
    changeAmountBtns.forEach(function (button) {
        button.addEventListener("click", function (event) {
            let buttontext = event.target.innerText;
            let amountElement = document.getElementById(`prd-quantity-${event.target.id}`);
            let amount = parseInt(amountElement.innerText);
            if (buttontext == ">") {
                amountElement.innerText = amount + 1;
            }
            else if (buttontext == "<" && amount > 1) {
                amountElement.innerText = amount - 1;
            }
        });
    });
    smallImages.forEach(function (img) {
        img.addEventListener('mouseenter', function () {
            mainImage.src = img.src;
        })
    })
})
function zoom(event) {
    const img = document.getElementById('image');
    const boundingRect = img.getBoundingClientRect();

    const mouseX = event.clientX - boundingRect.left;
    const mouseY = event.clientY - boundingRect.top;

    const scale = 1.5; // Adjust the zoom level

    const offsetX = mouseX / img.width * 100;
    const offsetY = mouseY / img.height * 100;

    img.style.transformOrigin = `${offsetX}% ${offsetY}%`; // Set transform origin

    img.style.transform = `scale(${scale})`;

    img.parentElement.addEventListener('mouseleave', function () {
        img.style.transform = 'scale(1)'; // Reset scale on mouse leave
    });
}