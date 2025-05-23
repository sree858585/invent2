<template>
    <div class="modal-overlay">
        <div class="modal confirmation">
            <button class="close-btn" @click="$emit('close')">&times;</button>
            <h3>⚠️ Confirm {{ roleType }} Access</h3>
            <p>
                Are you sure you want to
                <strong>{{ isGranting ? 'grant' : 'remove' }}</strong>
                {{ roleType.toLowerCase() }} access to
                <strong>{{ email }}</strong>?
            </p>
            <div class="button-group">
                <button class="btn-confirm" @click="$emit('confirm')">Yes, Confirm</button>
                <button class="btn-cancel" @click="$emit('close')">No, Cancel</button>
            </div>
        </div>
    </div>
</template>

<script>export default {
  name: "AccessConfirmModal",
  props: {
    email: String,
    isGranting: Boolean, // true = grant, false = remove
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
        background: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 2000;
    }

    .modal.confirmation {
        background: #fff;
        padding: 30px 20px;
        border-radius: 14px;
        max-width: 420px;
        width: 90%;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
        position: relative;
        border-top: 6px solid #ffc107;
        text-align: center;
        animation: fadeIn 0.3s ease;
    }

    .modal h3 {
        font-size: 22px;
        font-weight: 600;
        color: #ff9800;
        margin-bottom: 14px;
    }

    .modal p {
        font-size: 16px;
        color: #444;
        margin-bottom: 20px;
    }

    .close-btn {
        position: absolute;
        top: 12px;
        right: 16px;
        font-size: 22px;
        color: #999;
        background: none;
        border: none;
        cursor: pointer;
    }

    .button-group {
        display: flex;
        justify-content: center;
        gap: 20px;
        margin-top: 20px;
    }

    .btn-confirm {
        background-color: #4caf50;
        color: white;
        padding: 8px 20px;
        border-radius: 6px;
        border: none;
        font-weight: bold;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

    .btn-cancel {
        background-color: #eee;
        color: #333;
        padding: 8px 20px;
        border-radius: 6px;
        border: none;
        font-weight: bold;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

    .btn-confirm:hover {
        background-color: #388e3c;
    }

    .btn-cancel:hover {
        background-color: #ddd;
    }

    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
</style>