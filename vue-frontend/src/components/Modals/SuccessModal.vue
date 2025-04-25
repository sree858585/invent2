<template>
    <div class="overlay">
        <div class="modal">
            <button class="close-btn" @click="$emit('close')">&times;</button>

            <!-- ✅ Animation Slot -->
            <div class="lottie-wrapper">
                <!-- Replace with your Lottie component or animation here -->
                <img src="@/assets/Done.gif" alt="Success Animation" />
            </div>

            <h2>🎉 Registration Successful!</h2>
            <p>
                You will receive an email shortly with more details about this course.<br />
                📧 Sent to: <strong>{{ email }}</strong>
            </p>
            <button class="ok-btn" @click="$emit('close')">OK</button>
        </div>
    </div>
</template>

<script>export default {
    name: "SuccessModal",
    props: {
        email: {
            type: String,
            required: true
        }
    }
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
        padding: 30px 20px 24px;
        border-radius: 12px;
        text-align: center;
        max-width: 420px;
        width: 90%;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
        position: relative;
        font-family: 'Segoe UI', sans-serif;
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
        font-weight: bold;
    }

    .modal h2 {
        color: #388e3c;
        margin-top: 0;
        font-size: 22px;
    }

    .modal p {
        margin: 16px 0;
        font-size: 16px;
        color: #444;
    }

    .ok-btn {
        background: #388e3c;
        color: white;
        border: none;
        padding: 10px 24px;
        font-weight: 600;
        border-radius: 6px;
        font-size: 15px;
        cursor: pointer;
    }

        .ok-btn:hover {
            background: #2e7d32;
        }
    .lottie-wrapper {
        margin-bottom: 16px;
        display: flex;
        justify-content: center;
        align-items: center;
    }

        .lottie-wrapper img {
            max-width: 200px;
            max-height: 200px;
            object-fit: contain;
        }
</style>