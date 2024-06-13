const router = require('express').Router()
const jwt = require('jsonwebtoken')
const Pool = require('pg').Pool;
const dotenv = require("dotenv");

dotenv.config()

const pool = new Pool({
    user: process.env.PGUSER,
    host: process.env.PGHOST,
    database: process.env.PGDATABASE,
    password: process.env.PGPASSWORD,
    port: process.env.PGPORT,
})

const ACCESS_TOKEN_SECRET = process.env.ACCESS_TOKEN_SECRET;

//localhost:8080/api/process/
router.get('/', async (req, res) => {
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    try {
        jwt.verify(token, process.env.ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('select * from processes where email=$1', [decoded.user.email])
                .then(results => res.status(200).send(results.rows))
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(500).send(err)
    }
})

//localhost:8080/api/process/
router.post('/', async (req, res) => {
    if (contentTypeHeaderExists(req.headers)) return res.status(400).send("No 'Content-Type' Header delivered.")
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    const process = req.body
    try {
        jwt.verify(token, ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('insert into processes values ($1, $2, $3, $4)', [process.processName, decoded.user.email, process.displayedName, process.sec])
                .then(_ => res.status(201).send())
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(500).send(err)
    }
})

//localhost:8080/api/process/
router.put('/', async (req, res) => {
    if (contentTypeHeaderExists(req.headers)) return res.status(400).send("No 'Content-Type' Header delivered.")
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    const process = req.body
    try {
        jwt.verify(token, ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('UPDATE processes SET displayedName=$2, sec=$3 WHERE email=$1 and processName=$4', [decoded.user.email, process.displayedName, process.sec, process.processName])
                .then(_ => res.status(200).send())
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(500).send(err)
    }
})

//localhost:8080/api/process/getByDisplayedName/
router.get('/getByDisplayedName/:displayedName', async (req, res) => {
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    try {
        jwt.verify(token, process.env.ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('select * from processes where email=$1 and displayedName=$2', [decoded.user.email, req.params.displayedName])
                .then(results => res.status(200).send(results.rows))
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(500).send(err)
    }
})

//localhost:8080/api/process/{processName}/
router.get('/:processName', async (req, res) => {
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    const params = req.params
    try {
        jwt.verify(token, process.env.ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('select * from processes where email=$1 and processName=$2', [decoded.user.email, params.processName])
                .then(results => res.status(200).send(results.rows))
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(500).send(err)
    }
})

//localhost:8080/api/process/{processName}/
router.delete('/:processName', async (req, res) => {
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    const params = req.params

    try {
        jwt.verify(token, process.env.ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('DELETE from processes where email=$1 and processName=$2', [decoded.user.email, params.processName])
                .then(_ => res.status(200).send("Successfully Deleted"))
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(500).send(err)
    }
})

function contentTypeHeaderExists(header) {
    const keys = Object.keys(header)

    if (keys.includes("content-type")) return false
    return true
}

function authorizationHeaderExists(header) {
    const keys = Object.keys(header)

    if (keys.includes("authorization")) return false
    return true
}

module.exports = router