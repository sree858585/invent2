<template>
    <div class="modal-overlay">
        <div class="modal success">
            <h3>✅ {{ roleType }} Access Updated</h3>
            <p>
                <strong>{{ user.email }}</strong> has been successfully
                <span class="highlight">{{ action }}</span> {{ roleType.toLowerCase() }} access.
            </p>
            <div class="button-group">
                <button class="btn-primary" @click="$emit('close')">OK</button>
            </div>
        </div>
    </div>
</template>

<script>export default {
  props: {
    user: Object,
    action: String,
    roleType: {
      type: String,
      default: "Admin"
    }
  }
};</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background-color: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 999;
    }

    .modal.success {
        background: #fff;
        padding: 32px;
        border-radius: 14px;
        max-width: 400px;
        width: 90%;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
        text-align: center;
        border-top: 6px solid #4caf50;
        animation: fadeInScale 0.3s ease;
    }

        .modal.success h3 {
            color: #388e3c;
            margin-bottom: 10px;
        }

        .modal.success p {
            color: #444;
            font-size: 16px;
        }

        .modal.success .highlight {
            color: #388e3c;
            font-weight: bold;
        }

    .button-group {
        margin-top: 20px;
    }

    .btn-primary {
        background-color: #1976d2;
        color: white;
        padding: 8px 20px;
        border-radius: 6px;
        border: none;
        font-size: 14px;
        cursor: pointer;
    }

        .btn-primary:hover {
            background-color: #1565c0;
        }

    @keyframes fadeInScale {
        from {
            opacity: 0;
            transform: scale(0.9);
        }

        to {
            opacity: 1;
            transform: scale(1);
        }
    }
</style>