<template>
    <div class="modal-overlay">
        <div class="modal">
            <h3>Email Users</h3>
            <p>Send an email to participants of {{ course?.subjectTitle }}</p>
            <button class="btn-danger" @click="$emit('close')">Close</button>
        </div>
    </div>
</template>

<script>export default {
        props: ['course']
    };</script>