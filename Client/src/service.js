import axios from 'axios';

const apiUrl = "https://tasks-management-server-nnyn.onrender.com";
console.log('API URL!!:', apiUrl);

export default {
  get: async () => {
    const result = await axios.get(`${apiUrl}/`)
    return result.data;
  },

  getTasks: async () => {
    const result = await axios.get(`${apiUrl}/tasks`)
    return result.data;
  },

  getTaskById: async (id) => {
    const result = await axios.get(`${apiUrl}/tasks/${id}`)
    return result.data;
  },

  addTask: async (name) => {
    console.log('API URL!!:', apiUrl);
    console.log('REACT_APP_API_URL:', process.env.REACT_APP_API_URL);
    console.log('addTask', name)
    const result = await axios.post(`${apiUrl}/tasks`, { name })
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    console.log('setCompleted', { id, isComplete })
    const result = await axios.put(`${apiUrl}/tasks/${id}`, { isComplete })
    return result.data;
  },

  deleteTask: async (id) => {
    console.log('deleteTask', id)
    const result = await axios.delete(`${apiUrl}/tasks/${id}`)
    return result.data;
  }
}