module.exports = (req, res, next) => {
    if(/\.(ejs|psd|ts|scss)$/.test(req.url)){        
        res.status(404).send('Not found');
        return;
    }
    next();
};