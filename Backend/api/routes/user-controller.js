const bcrypt = require('bcrypt')
const router = require('express').Router()
const jwt = require('jsonwebtoken')
const dotenv = require('dotenv')
const hash = require('object-hash');

const Pool = require('pg').Pool;

dotenv.config()

const pool = new Pool({
    user: process.env.PGUSER,
    host: process.env.PGHOST,
    database: process.env.PGDATABASE,
    password: process.env.PGPASSWORD,
    port: process.env.PGPORT,
})

//localhost:8080/api/user/register
router.post('/register', async (req, res) => {
    if (contentTypeHeaderExists(req.headers)) return res.status(400).send("No 'Content-Type' Header delivered.")

    const hashedPassword = await bcrypt.hash(req.body.password, 10)

    pool.query('SELECT * FROM users where email=$1', [req.body.email])
        .then(results => {
            if (results.rows.length === 0) {
                try {
                    const user = {
                        email: req.body.email,
                        password: hashedPassword,
                        firstName: req.body.firstName,
                        surname: req.body.surname,
                        role: "default",
                    }
                    const tokenUser = {
                        email: req.body.email,
                        password: req.body.password
                    }

                    const accessToken = jwt.sign({user: tokenUser}, process.env.ACCESS_TOKEN_SECRET, {expiresIn: '10m'})
                    const refreshToken = jwt.sign({user: tokenUser}, process.env.REFRESH_TOKEN_SECRET, {expiresIn: "10h"})

                    pool.query('INSERT INTO users VALUES ($1, $2, $3, $4, $5)', [user.email, user.password, user.firstName, user.surname, user.role])
                        .then(_ => {
                            res.status(201).send({accessToken, refreshToken})
                        })
                        .catch(err => res.status(500).send(err))
                } catch (err) {
                    return res.status(500).send(err)
                }
            } else {
                return res.status(400).send("Email is already being used")
            }
        })
        .catch(err => res.status(500).send(err))
})

//localhost:8080/api/user/login
router.post('/login', async (req, res) => {
    if (contentTypeHeaderExists(req.headers)) return res.status(400).send("No 'Content-Type' Header delivered.")

    const user = {
        email: req.body.email,
        password: req.body.password
    }
    pool.query('select * from users where email=$1', [user.email])
        .then(async results => {
            if (results.rows.length === 1) {
                try {
                    if (await bcrypt.compare(user.password, results.rows[0].password)) {                // Successfully logged in
                        const accessToken = jwt.sign({user}, process.env.ACCESS_TOKEN_SECRET, {expiresIn: '5m'})
                        const refreshToken = jwt.sign({user}, process.env.REFRESH_TOKEN_SECRET, {expiresIn: "10h"})

                        return res.status(200).send({accessToken, refreshToken})
                    } else                                                                          // Unsuccessful
                        return res.status(400).send("Wrong password")
                } catch (err) {                                                                           // Something went wrong
                    return res.status(500).send(err)
                }
            } else return res.status(400).send("Email is not yet registered. / No user found")
        })
        .catch(err => res.status(500).send(err))
})

//localhost:8080/api/user/login
router.get('/token', async (req, res) => {
    if (contentTypeHeaderExists(req.headers)) return res.status(400).send("No 'Content-Type' Header delivered.")
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)

    pool.query('select * from denylist')
        .then(results => {
            const hashedToken = hash(token)

            for (const row of results.rows) if (row.hashedtoken === hashedToken) return res.status(400).send('Token has been blacklisted. Please register to get a new refresh token!')

            try {
                jwt.verify(token, process.env.REFRESH_TOKEN_SECRET, async function (err, decoded) {
                    if (err) return res.status(300).send(err)

                    const user = {
                        email: decoded.user.email, password: decoded.user.password
                    }

                    const accessToken = jwt.sign({user}, process.env.ACCESS_TOKEN_SECRET, {expiresIn: '5m'})
                    return res.status(200).send({accessToken})
                })
            } catch (err) {
                return res.status(400).send(err)
            }
        })
        .catch(err => res.status(500).send(err))
})

//localhost:8080/api/user/logout
router.delete('/logout', async (req, res) => {
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No Authorization Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    try {
        jwt.verify(token, process.env.REFRESH_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            const hashedToken = hash(token)
            pool.query('insert into denyList values ($1, $2)', [hashedToken, decoded.exp])
                .then(_ => res.status(200).send("Invalidated User Tokens"))
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(400).send(err)
    }
})

//localhost:8080/api/user/
router.get('/', async (req, res) => {
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    try {
        jwt.verify(token, process.env.ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('select * from users where email=$1', [decoded.user.email])
                .then(results => {
                    if (results.rows.length === 1) {
                        const user = {
                            email: results.rows[0].email,
                            password: results.rows[0].password,
                            firstName: results.rows[0].firstname,
                            surname: results.rows[0].surname,
                            role: results.rows[0].role,
                        }

                        return res.status(200).send(user)
                    }
                    else return res.status(400).send({empty: "true"})
                })
                .catch(err => res.status(500).send(err))
        })
    } catch (err) {
        return res.status(500).send(err)
    }
})

//localhost:8080/api/user/
router.put('/', async (req, res) => {
    if (contentTypeHeaderExists(req.headers)) return res.status(400).send("No 'Content-Type' Header delivered.")
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)

    try {
        jwt.verify(token, process.env.ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            let user = decoded.user

            const hashedPassword = await bcrypt.hash(req.body.password, 10)
            const newUser = {
                email: req.body.email, password: hashedPassword, firstName: req.body.firstName, surname: req.body.surname, role: req.body.role
            }

            pool.query('select * from users where email=$1', [user.email])
                .then(async results => {
                    if (results.rows.length === 1) {
                        try {
                            if (await bcrypt.compare(user.password, results.rows[0].password)) {
                                pool.query('UPDATE users SET email=$1, password=$2, firstName=$3, surname=$4, role=$5 where email=$6',
                                    [newUser.email, newUser.password, newUser.firstName, newUser.surname, newUser.role, user.email])
                                    .then(_ => res.status(200).send("Successfully updated user"))
                                    .catch(err => res.status(500).send(err))
                            } else return res.status(400).send("Wrong password")
                        } catch (err) {
                            return res.status(500).send(err)
                        }
                    } else return res.status(400).send("No user with this email found!")
                })
                .catch(err => res.status(500).send(err))
        })
    } catch {
        return res.status(500).send()
    }

})

//localhost:8080/api/user/
router.delete('/', async (req, res) => {
    if (contentTypeHeaderExists(req.headers)) return res.status(400).send("No 'Content-Type' Header delivered.")
    if (authorizationHeaderExists(req.headers)) return res.status(400).send("No 'Authorization' Header delivered.")

    const auth = req.header('authorization')
    const token = auth.substring(7)
    try {
        jwt.verify(token, process.env.ACCESS_TOKEN_SECRET, async function (err, decoded) {
            if (err) return res.status(300).send(err)

            pool.query('Delete from users where email=$1', [decoded.user.email])
                .then(_ => res.status(200).send("Successfully deleted user"))
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

// function tokenExpired(token) {
//     if (Date.now() >= token.exp * 1000) return true
//     return false
// }

module.exports = router