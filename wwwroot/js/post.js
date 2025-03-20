//const connection = new signalR.HubConnectionBuilder()
//    .withUrl("/postHub")
//    .withAutomaticReconnect()
//    .build();

//connection.start().then(() => {
//    console.log("Connected to PostHub");
//});

//document.querySelectorAll(".like-button").forEach(btn => {
//    btn.addEventListener("click", async () => {
//        const postId = btn.dataset.postId;
//        await fetch(`/Posts/Like?id=${postId}`, { method: "POST" });
//    });
//});

//connection.on("ReceiveLike", (postId, likeCount) => {
//    const likeBtn = document.querySelector(`.like-button[data-post-id="${postId}"]`);
//    if (likeBtn) {
//        likeBtn.querySelector(".like-count").textContent = likeCount;
//    }
//});

//connection.on("ReceiveComment", (postId, commentHtml) => {
//    const commentContainer = document.querySelector(`#comments-${postId} .comments-container`);
//    if (commentContainer) {
//        commentContainer.innerHTML += commentHtml;
//    }
//});
