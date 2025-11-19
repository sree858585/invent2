<template>
    <div class="calendar-wrapper">
        <h1 class="calendar-title">Training Calendar</h1>

        <FullCalendar class="fc-theme-standard"
                      :options="calendarOptions" />
    </div>
</template>

<script>import FullCalendar from "@fullcalendar/vue3";
    import dayGridPlugin from "@fullcalendar/daygrid";
    import timeGridPlugin from "@fullcalendar/timegrid";
    import interactionPlugin from "@fullcalendar/interaction";

    export default {
        name: "TrainingCalendar",
        components: { FullCalendar },
        data() {
            return {
                calendarOptions: {
                    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
                    initialView: "dayGridMonth",
                    headerToolbar: {
                        left: "prev,next today",
                        center: "title",
                        right: "dayGridMonth,timeGridWeek,timeGridDay"
                    },
                    height: "auto",
                    selectable: true,
                    editable: false,
                    events: [
                        // SAMPLE DATA (you can replace later)
                        { title: "HIV Training Bootcamp", date: "2025-11-05" },
                        { title: "PrEP Guidelines", date: "2025-11-12" },
                        { title: "Buprenorphine Workshop", date: "2025-11-18" }
                    ]
                }
            };
        }
    };</script>

<style scoped>
    .calendar-wrapper {
        width: 100%;
        padding: 30px;
        box-sizing: border-box;
    }

    .calendar-title {
        text-align: center;
        font-size: 2rem;
        font-weight: bold;
        color: #6e528d;
        margin-bottom: 20px;
    }

    /* Full width calendar override */
    :deep(.fc) {
        background: #ffffff;
        border-radius: 12px;
        padding: 20px;
        box-shadow: 0 4px 18px rgba(0,0,0,0.08);
    }

    /* Header style */
    :deep(.fc-toolbar-title) {
        font-size: 1.4rem;
        font-weight: 600;
        color: #444;
    }

    /* Buttons */
    :deep(.fc-button) {
        background: #6e528d !important;
        border: none !important;
        color: white !important;
        padding: 6px 14px !important;
        border-radius: 6px !important;
        font-size: 0.9rem !important;
        cursor: pointer !important;
    }

    :deep(.fc-button:hover) {
        background: #593d72 !important;
    }
</style>