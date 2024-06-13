const express = require('express')
const dotenv = require('dotenv')

const userRoute = require('./routes/user-controller.js')
const processRoute = require('./routes/process-controller.js')
const Pool = require("pg").Pool;

dotenv.config()

const app = express()

//Middleware
app.use(express.json())

//Route Middleware
app.use('/api/user', userRoute)
app.use('/api/process', processRoute)

app.listen(process.env.PORT, () => console.log('Server is running on PORT ' + process.env.PORT))
