import axios from "axios";

const API_BASE_URL = "https://localhost:5001/api/course";

export const CourseService = {
    getCourses() {
        return axios.get(`${API_BASE_URL}/all`);
    },
    getCoursesByFormat(format) {
        return axios.get(`${API_BASE_URL}/Format/${format}`);
    },
};
