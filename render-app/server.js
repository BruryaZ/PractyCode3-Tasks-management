const express = require('express');
const axios = require('axios');
const helmet = require('helmet');

const app = express();
const PORT = process.env.PORT || 3000;

const API_KEY = 'rnd_gdjXrpLnJt4jis4cTXK96fiqG2PO';

// Use helmet to set security-related HTTP headers
app.use(helmet());

// Set CSP headers
app.use((req, res, next) => {
    res.setHeader("Content-Security-Policy", "default-src 'self'; script-src 'self' https://netfree.link");
    next();
});

app.get('/', (req, res) => {
    res.send('Welcome to the home page!');
});

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