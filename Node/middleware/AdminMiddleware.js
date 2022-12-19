
module.exports = (req, res, next) => {
    if(req.url.startsWith('/401')){
        next();
        return;
    }
    if(req.isGuest || req.user.IsAdmin === false){
        res.status(401).redirect('/');
        return;
    }
    next();
};