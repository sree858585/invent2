<template>
    <div class="modal-overlay">
        <div class="modal">
            <h3>Add User to Course</h3>
            <p>Course: {{ course?.subjectTitle }}</p>
            <button class="btn-danger" @click="$emit('close')">Close</button>
        </div>
    </div>
</template>

<script>export default {
        props: ['course']
    };</script>