document.addEventListener("DOMContentLoaded", function () {
    var overlay = document.getElementById("overlay");
    var popup = document.getElementById("email-popup");
    var closeBtn = $(".close-form");
    var emailBtn = document.getElementById("email-btn");
    var emailInput = $('#email-input');
    overlay.style.display = "block";
    popup.style.display = "block";

    emailBtn.addEventListener("click", function () {
        var email = emailInput.val();
        if (email.trim() === '') {
            $("#error").text("Email can not be empty");
            $("#error").show(); // Display the danger label
            return;
        }
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            $("#error").text("Invalid email format");
            $("#error").show(); // Display the danger label with different message
            return;
        }
        $.ajax({
            url: '/Email/SendEmailAction',
            data: { email: emailInput.val() },
            success: function (response) {
                console.log(response);
                if (response.success) {
                overlay.style.display = "none";
                popup.style.display = "none";
                superconfetti();
                }
                else {
                    $("#error").text(response.message);
                    $("#error").show(); // Display the danger label with different message
                }
            },
            error: function (error) {
                console.error('Error:', error);
            }
        });
    });

    // Close the popup when clicking the close button
    closeBtn.each(function () {
        $(this).on("click", function () {
            overlay.style.display = "none";
            popup.style.display = "none";
        });
    });
    // Close the popup when clicking outside the popup area
    overlay.addEventListener("click", function () {
        overlay.style.display = "none";
        popup.style.display = "none";
    });

    // Prevent the overlay from closing the popup when clicking inside the popup
    popup.addEventListener("click", function (event) {
        event.stopPropagation();
    });
});
function superconfetti() {

    var count = 200;
    var defaults = {
        origin: { y: 0.7 }
    };

    function fire(particleRatio, opts) {
        confetti({
            ...defaults,
            ...opts,
            particleCount: Math.floor(count * particleRatio)
        });
    }

    fire(0.25, {
        spread: 26,
        startVelocity: 55,
    });
    fire(0.2, {
        spread: 60,
    });
    fire(0.35, {
        spread: 100,
        decay: 0.91,
        scalar: 0.8
    });
    fire(0.1, {
        spread: 120,
        startVelocity: 25,
        decay: 0.92,
        scalar: 1.2
    });
    fire(0.1, {
        spread: 120,
        startVelocity: 45,
    });
}