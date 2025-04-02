<template>
    <div class="modal-overlay">
        <div class="modal">
            <h3>Cancel Course</h3>
            <p>Are you sure you want to cancel {{ course?.subjectTitle }}?</p>
            <button class="btn-danger" @click="$emit('close')">Close</button>
        </div>
    </div>
</template>

<script>export default {
        props: ['course']
    };</script>