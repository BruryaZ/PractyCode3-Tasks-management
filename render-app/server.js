const express = require('express');
const axios = require('axios');

const app = express();
const PORT = process.env.PORT || 3000;


const API_KEY = 'rnd_gdjXrpLnJt4jis4cTXK96fiqG2PO';

app.get('/services', async (req, res) => {
    try {
        const response = await axios.get('https://api.render.com/v1/services', {
            headers: {
                'Authorization': `Bearer ${API_KEY}`
            }
        });
        res.json(response.data);
    } catch (err) {
        console.error(err);
        res.status(500).send('Error retrieving services');
    }
});

app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});