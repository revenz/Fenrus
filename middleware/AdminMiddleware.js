
module.exports = (req, res, next) => {
    if(req.isGuest || req.user.IsAdmin === false){
        res.status(401).redirect('/');
        return;
    }
    next();
};