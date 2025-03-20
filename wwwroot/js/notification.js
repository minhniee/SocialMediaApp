// Notification handling with SignalR
$(document).ready(() => {
    // Create connection
    const connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build()

    // Start the connection
    startConnection()

    function startConnection() {
        connection
            .start()
            .then(() => {
                console.log("SignalR Connected")
            })
            .catch((err) => {
                console.error("SignalR Connection Error: ", err)
                // Retry connection after 5 seconds
                setTimeout(startConnection, 5000)
            })
    }

    // Handle receiving a new notification
    connection.on("ReceiveNotification", (notification) => {
        // Add the new notification to the dropdown
        addNotificationToDropdown(notification)

        // Show a toast notification
        showToastNotification(notification)
    })

    // Handle updating the unread count
    connection.on("UpdateUnreadCount", (count) => {
        updateUnreadBadge(count)
    })

    // Function to add a notification to the dropdown
    function addNotificationToDropdown(notification) {
        // Create the notification HTML
        const notificationHtml = createNotificationHtml(notification)

        // Get the notification dropdown
        const dropdown = $(".notification-dropdown")

        // Check if there's a "No notifications" message
        const noNotificationsItem = dropdown.find(".dropdown-item.text-center.py-3")
        if (noNotificationsItem.length) {
            // Remove the "No notifications" message
            noNotificationsItem.remove()
        }

        // Add the new notification at the top of the list
        dropdown.find("h6.dropdown-header").after(notificationHtml)
    }

    // Function to create notification HTML
    function createNotificationHtml(notification) {
        const profilePic = notification.sourceUser?.profilePictureUrl || "/images/default-profile.png"

        return `
            <li>
                <a class="dropdown-item bg-light" href="#">
                    <div class="d-flex align-items-center">
                        <img src="${profilePic}" class="rounded-circle me-2" width="32" height="32" alt="Profile picture">
                        <div class="small">
                            <div>${notification.message}</div>
                            <div class="text-muted">${notification.timeAgo}</div>
                        </div>
                    </div>
                </a>
            </li>
        `
    }

    // Function to show a toast notification
    function showToastNotification(notification) {
        const profilePic = notification.sourceUser?.profilePictureUrl || "/images/default-profile.png"

        // Create toast container if it doesn't exist
        if ($("#toast-container").length === 0) {
            $("body").append('<div id="toast-container" class="position-fixed top-0 end-0 p-3" style="z-index: 1080;"></div>')
        }

        // Create a unique ID for the toast
        const toastId = `toast-${Date.now()}`

        // Create the toast HTML
        const toastHtml = `
            <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="5000">
                <div class="toast-header">
                    <img src="${profilePic}" class="rounded me-2" width="20" height="20" alt="Profile picture">
                    <strong class="me-auto">New Notification</strong>
                    <small>${notification.timeAgo}</small>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${notification.message}
                </div>
            </div>
        `

        // Add the toast to the container
        $("#toast-container").append(toastHtml)

        // Initialize and show the toast
        const toastElement = document.getElementById(toastId)
        const toast = new bootstrap.Toast(toastElement)
        toast.show()

        // Remove the toast from the DOM after it's hidden
        $(toastElement).on("hidden.bs.toast", function () {
            $(this).remove()
        })
    }

    // Function to update the unread badge
    function updateUnreadBadge(count) {
        const badge = $("#notificationsDropdown .badge")

        if (count > 0) {
            // If badge doesn't exist, create it
            if (badge.length === 0) {
                $("#notificationsDropdown").append(`
                    <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                        ${count > 99 ? "99+" : count}
                        <span class="visually-hidden">unread notifications</span>
                    </span>
                `)
            } else {
                // Update existing badge
                badge.text(count > 99 ? "99+" : count)
                badge.removeClass("d-none")
            }
        } else {
            // Hide the badge if count is 0
            badge.addClass("d-none")
        }
    }

    // Handle marking notifications as read
    $(document).on("click", ".mark-read-btn", function () {
        const notificationId = $(this).data("notification-id")

        $.ajax({
            url: "/Notifications/MarkAsRead",
            type: "POST",
            data: { id: notificationId },
            success: () => {
                $(`#notification-${notificationId}`).removeClass("bg-light")
                $(`#notification-${notificationId} .mark-read-btn`).remove()
            },
        })
    })
})

