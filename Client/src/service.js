import axios from 'axios';

const apiUrl = process.env.REACT_APP_API_URL
console.log('API URL!!:', apiUrl);

export default {
  get: async () => {
    const result = await axios.get(`${apiUrl}/`)
    return result.data;
  },

  getTasks: async () => {
    const result = await axios.get(`${apiUrl}/items`)
    return result.data;
  },

  getTaskById: async (id) => {
    const result = await axios.get(`${apiUrl}/items/${id}`)
    return result.data;
  },

  addTask: async (name) => {
    console.log('addTask', name)
    const result = await axios.post(`${apiUrl}/items`, { name })
    return result.data;
  },

  setCompleted: async (id, isComplete) => {
    console.log('setCompleted', { id, isComplete })
    const result = await axios.put(`${apiUrl}/items/${id}`, { isComplete })
    return result.data;
  },

  deleteTask: async (id) => {
    console.log('deleteTask', id)
    const result = await axios.delete(`${apiUrl}/items/${id}`)
    return result.data;
  }
}