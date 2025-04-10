<template>
    <div class="modal-overlay">
        <div class="modal">
            <h2>Archive Instructor</h2>
            <p>Are you sure you want to archive <strong>{{ instructor.name }}</strong>?</p>
            <div class="button-group">
                <button class="btn-danger" @click="confirmArchive">Yes, Archive</button>
                <button class="btn-secondary" @click="$emit('close')">Cancel</button>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";

    export default {
        props: ["instructor"],
        emits: ["archived", "close"],
        methods: {
            async confirmArchive() {
                try {
                    await apiClient.put(`/InstructorManagement/archive/${this.instructor.instructorSysId}`);
                    alert("Instructor archived successfully!");
                    this.$emit("archived");
                    this.$emit("close");
                } catch (err) {
                    console.error("Error archiving instructor:", err);
                    alert("Failed to archive instructor.");
                }
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

    .modal {
        background-color: white;
        padding: 30px;
        border-radius: 14px;
        width: 440px;
        text-align: center;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
    }

    h2 {
        font-size: 22px;
        margin-bottom: 20px;
    }

    p {
        margin-bottom: 30px;
    }

    .button-group {
        display: flex;
        justify-content: center;
        gap: 16px;
    }

    .btn-danger {
        background-color: #e74c3c;
        color: white;
        padding: 10px 20px;
        border-radius: 8px;
        cursor: pointer;
        border: none;
    }

    .btn-secondary {
        background-color: #ccc;
        padding: 10px 20px;
        border-radius: 8px;
        cursor: pointer;
        border: none;
    }
</style>