import { createApp } from 'vue';
import App from './App.vue';
import router from './router'; // Import the router
import { QuillEditor } from 'vue3-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css'
import VueApexCharts from "vue3-apexcharts";





const app = createApp(App);
app.component('QuillEditor', QuillEditor);
app.use(VueApexCharts);

app.use(router); // Use the router
app.mount('#app');
