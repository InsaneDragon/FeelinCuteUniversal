document.addEventListener("DOMContentLoaded", function () {
    var hiddencartcount = document.getElementById('hidden-cart-count').innerText;
    var cartcount = document.querySelectorAll('.cart-count');
    cartcount.forEach(count => count.innerText = hiddencartcount);
    var hiddenlikecount = document.getElementById('hidden-like-count').innerText;
    var likeCount = document.querySelectorAll('.like-count');
    likeCount.forEach(count => count.innerText = hiddenlikecount);
});
