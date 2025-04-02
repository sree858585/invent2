<template>
    <div class="modal-overlay">
        <div class="modal">
            <h2>Confirm Delete</h2>
            <p>Are you sure you want to delete the training title "<strong>{{ title.courseTitle }}</strong>"?</p>
            <div class="button-group">
                <button class="btn-danger" @click="confirmDelete">Yes, Delete</button>
                <button class="btn-secondary" @click="$emit('close')">Cancel</button>
            </div>
        </div>
    </div>
</template>

<script>export default {
        props: ['title'],
        emits: ['close', 'deleted'],
        methods: {
            confirmDelete() {
                this.$emit('deleted', this.title.subjectSysId);
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
        z-index: 999;
    }

    .modal {
        background-color: white;
        padding: 24px;
        border-radius: 10px;
        width: 400px;
        text-align: center;
    }

    .button-group {
        display: flex;
        justify-content: center;
        gap: 12px;
        margin-top: 20px;
    }

    .btn-danger {
        background-color: #e74c3c;
        color: white;
        border: none;
        padding: 10px 16px;
        border-radius: 6px;
        cursor: pointer;
    }

    .btn-secondary {
        background-color: #ccc;
        color: black;
        border: none;
        padding: 10px 16px;
        border-radius: 6px;
        cursor: pointer;
    }
</style>