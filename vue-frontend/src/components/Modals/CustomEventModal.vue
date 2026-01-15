<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal-content">
            <div class="modal-banner">
                <h2 class="title">{{ title }}</h2>
                <button class="close-btn" @click="$emit('close')">&times;</button>
            </div>

            <div class="modal-body">
                <div class="row">
                    <div><strong>Date:</strong> {{ formatDate(start) }}</div>
                    <div v-if="end"><strong>End:</strong> {{ formatDate(end) }}</div>
                </div>

                <div v-if="location" class="row">
                    <div><strong>Location:</strong> {{ location }}</div>
                </div>

                <div v-if="shortDescription" class="section">
                    <h4>Summary</h4>
                    <p>{{ shortDescription }}</p>
                </div>

                <div v-if="longDescription" class="section">
                    <h4>Description</h4>
                    <p style="white-space: pre-wrap">{{ longDescription }}</p>
                </div>

                <div v-if="linkUrl" class="section">
                    <h4>Link</h4>
                    <a :href="linkUrl" target="_blank" rel="noopener">{{ linkUrl }}</a>
                </div>

                <div class="actions">
                    <button class="btn-secondary" @click="$emit('close')">Close</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>export default {
        name: "CustomEventModal",
        props: {
            event: { type: Object, required: true },
        },
        computed: {
            title() {
                return this.event?.title || this.event?.name || "Event";
            },
            start() {
                return this.event?.start || this.event?.startDate || this.event?.startStr || this.event?.startUtc;
            },
            end() {
                return this.event?.end || this.event?.endDate || this.event?.endStr || this.event?.endUtc;
            },
            location() {
                return this.event?.location || "";
            },
            shortDescription() {
                return this.event?.shortDescription || this.event?.description || "";
            },
            longDescription() {
                return this.event?.longDescription || this.event?.details || "";
            },
            linkUrl() {
                return this.event?.linkUrl || this.event?.url || "";
            },
        },
        methods: {
            formatDate(d) {
                if (!d) return "N/A";
                try {
                    return new Date(d).toLocaleString();
                } catch {
                    return String(d);
                }
            },
        },
    };</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1200;
    }

    .modal-content {
        background: #fff;
        width: 70vw;
        max-width: 920px;
        max-height: 90vh;
        overflow-y: auto;
        border-radius: 12px;
        box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
        font-family: "Segoe UI", sans-serif;
    }

    .modal-banner {
        padding: 18px 22px;
        border-bottom: 1px solid #e5e7eb;
        display: flex;
        align-items: center;
        gap: 12px;
        background: #f7f7fb;
    }

    .title {
        margin: 0;
        font-size: 20px;
        font-weight: 700;
        color: #2a2a2a;
        flex: 1;
    }

    .close-btn {
        width: 40px;
        height: 40px;
        border-radius: 999px;
        border: 1px solid #ddd;
        background: #fff;
        font-size: 26px;
        cursor: pointer;
    }

    .modal-body {
        padding: 18px 22px 22px;
    }

    .row {
        display: flex;
        gap: 18px;
        flex-wrap: wrap;
        margin-bottom: 12px;
        color: #333;
    }

    .section {
        margin-top: 14px;
    }

        .section h4 {
            margin: 0 0 8px;
            font-size: 16px;
            color: #333;
        }

    .actions {
        display: flex;
        justify-content: flex-end;
        margin-top: 18px;
    }

    .btn-secondary {
        padding: 10px 18px;
        border: none;
        border-radius: 8px;
        background: #e5e7eb;
        cursor: pointer;
        font-weight: 600;
    }
</style>