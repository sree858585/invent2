<template>
    <div class="overlay">
        <div class="modal">
            <button class="close-btn" @click="$emit('close')">&times;</button>

            <div class="lottie-wrapper">
                <img src="@/assets/Girl.gif" alt="Warning" />
            </div>

            <h2>⚠️ Confirm Drop</h2>
            <p>Are you sure you want to drop this course?</p>

            <div class="button-group">
                <button class="confirm-btn" @click="$emit('confirm-drop')">Yes, Drop</button>
                <button class="cancel-btn" @click="$emit('close')">No, Cancel</button>
            </div>
        </div>
    </div>
</template>

<script>export default {
        name: "DropCourseConfirmModal",
    };</script>

<style scoped>
    .overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 2000;
    }

    .modal {
        background: #fff;
        padding: 24px 20px;
        border-radius: 12px;
        text-align: center;
        max-width: 400px;
        width: 90%;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
        position: relative;
    }

    .close-btn {
        position: absolute;
        top: 12px;
        right: 16px;
        font-size: 24px;
        color: #999;
        background: none;
        border: none;
        cursor: pointer;
    }

    h2 {
        color: #d32f2f;
        margin-top: 0;
        font-size: 20px;
    }

    p {
        color: #444;
        font-size: 16px;
        margin-bottom: 20px;
    }

    .button-group {
        display: flex;
        justify-content: space-around;
        gap: 10px;
    }

    .confirm-btn {
        background-color: #d32f2f;
        color: white;
        padding: 8px 18px;
        border-radius: 6px;
        font-weight: bold;
        border: none;
    }

    .cancel-btn {
        background-color: #eee;
        color: #333;
        padding: 8px 18px;
        border-radius: 6px;
        border: none;
        font-weight: bold;
    }

    .confirm-btn:hover {
        background-color: #b71c1c;
    }

    .cancel-btn:hover {
        background-color: #ddd;
    }

    .lottie-wrapper img {
        max-width: 160px;
        margin-bottom: 10px;
    }
</style>