
module.exports = (req, res, next) => {
    if(req.user.IsAdmin === false){
        res.status(401).redirect('/');
        return;
    }
    next();
};