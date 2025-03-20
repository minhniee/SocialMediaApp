// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(() => {
  // Handle like button clicks
  $(".like-button").on("click", function () {
    const button = $(this)
    const postId = button.data("post-id")

    $.ajax({
      url: "/Posts/ToggleLike",
      type: "POST",
      data: { postId: postId },
      success: (response) => {
        if (response.success) {
          const likeCount = button.find(".like-count")
          likeCount.text(response.likeCount)

          if (response.liked) {
            button.removeClass("btn-outline-primary").addClass("btn-primary")
            button.data("liked", "true")
          } else {
            button.removeClass("btn-primary").addClass("btn-outline-primary")
            button.data("liked", "false")
          }
        }
      },
    })
  })

  // Initialize tooltips
  var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
  var tooltipList = tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl))
})

