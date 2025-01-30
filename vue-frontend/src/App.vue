<template>
    <div id="app">
        <!-- Header should be at the top, aligned to the left -->
        <SiteHeader />

        <div class="main-container">
            <SideNav /> <!-- Sidebar on the left -->

            <div class="content">
                <router-view /> <!-- Page content -->
            </div>
        </div>

        <!-- Footer should be at the bottom, spanning full width -->
        <SiteFooter />
    </div>
</template>

<script>import SideNav from "@/components/SideNav.vue";
    import SiteHeader from "@/components/Header.vue";
    import SiteFooter from "@/components/Footer.vue";

    export default {
        name: "App",
        components: {
            SideNav,
            SiteHeader,
            SiteFooter,
        },
    };</script>

<style>
    /* Overall app layout */
    /* Overall app layout */
    #app {
        display: flex;
        flex-direction: column;
        height: 100vh; /* Full viewport height */
    }

    /* Ensure header spans full width and aligns content to the left */
    .SiteHeader {
        width: 100%;
        padding: 10px 20px;
        display: flex;
        align-items: center;
        background-color: #fff; /* Adjust background if needed */
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    /* Container for sidebar and main content */
    .main-container {
        display: flex;
        flex: 1; /* This makes the main container take the remaining height */
        overflow: hidden; /* Prevent overflow */
    }

    /* Sidebar styles */
    .SideNav {
        width: 250px; /* Fixed width for the sidebar */
        flex-shrink: 0; /* Do not shrink */
    }

    /* Main content area */
    .content {
        flex: 1; /* This will take the remaining space */
        padding: 20px;
        overflow-y: auto; /* Enable scrolling if contents are large */
    }

    /* Footer should be at the bottom and span full width */
    .SiteFooter {
        width: 100%; /* Full width */
        padding: 15px 20px;
        text-align: center;
        background-color: #000; /* Set to black (or your design color) */
        color: white;
    }
</style>
