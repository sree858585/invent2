<template>
    <div id="app">
        <!-- Header -->
        <SiteHeader />
        <MainHeader @show-login="showLoginModal = true" />
        <!-- Layout container -->
        <div class="main-container">
            <!-- Sidebar -->
            <SideNav @show-login="showLoginModal = true" />

            <!-- Main content -->
            <div class="content">

                <router-view />

            </div>

            <HelpAssistant />
        </div>

        <!-- Login Modal -->
        <LoginComponent v-if="showLoginModal"
                        @login-success="handleLoginSuccess"
                        @show-register="handleShowRegister"
                        @close="showLoginModal = false" />

        <!-- Registration Modal (optional) -->
        <RegistrationModal v-if="showRegistrationForm"
                           @close="showRegistrationForm = false" />
    </div>
</template>

<script>import SideNav from "@/components/SideNav.vue";
    import SiteHeader from "@/components/Header.vue";
    import LoginComponent from "@/components/LoginComponent.vue";
    import RegistrationModal from "@/components/RegistrationModal.vue";
    import eventBus from "@/eventBus.js";
    import MainHeader from "@/components/MainHeader.vue";
    import HelpAssistant from "@/components/HelpAssistant.vue";

    export default {
        name: "App",
        components: {
            SideNav,
            SiteHeader,
            LoginComponent,
            RegistrationModal,
            MainHeader,
                HelpAssistant
        },
        data() {
            return {
                showLoginModal: false,
                showRegistrationForm: false,
            };
        },
        created() {
            eventBus.on("auth-change", this.refreshState);
        },
        unmounted() {
            eventBus.off("auth-change", this.refreshState);
        },
        methods: {
            handleLoginSuccess(userData) {
                if (!userData || !userData.userId) {
                    alert("⚠️ Login response is invalid.");
                    return;
                }

                // Store login info
                localStorage.setItem("userId", userData.userId);
                localStorage.setItem("userName", `${userData.firstName} ${userData.lastName}`);
                localStorage.setItem("jwtToken", userData.token);

                this.showLoginModal = false;
                eventBus.emit("auth-change");
            },
            handleShowRegister() {
                this.showRegistrationForm = true;
                this.showLoginModal = false;
            },
            refreshState() {
                this.$forceUpdate(); // Force SideNav to update based on login state
            },
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
        flex: 1;
        padding: 0 20px 20px; /* ⬅️ was 20px; remove the top padding */
        overflow-y: auto;
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
