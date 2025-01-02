<template>
    <div id="app">
        <SideNav /> <!-- Side navigation rendered globally -->
        <div class="content">
            <router-view /> <!-- This is where pages/components are rendered -->
        </div>
    </div>
</template>

<script>import SideNav from "@/components/SideNav.vue";

    export default {
        name: "App",
        components: {
            SideNav,
        },
    };</script>

<style>
    #app {
        display: flex;
        font-family: Avenir, Helvetica, Arial, sans-serif;
    }

    .content {
        flex: 1;
        padding: 20px;
    }
</style>
