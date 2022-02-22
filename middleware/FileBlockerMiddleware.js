module.exports = (req, res, next) => {
    console.log('############## req.url', req.url);
    if(/\.(ejs|psd|ts|scss)$/.test(req.url)){        
        res.status(404).send('Not found');
        return;
    }
    next();
};