const gulp = require("gulp");
const browserify = require("browserify");
const babelify = require("babelify");
const source = require("vinyl-source-stream");
const buffer = require("vinyl-buffer");
const uglify = require("gulp-uglify");
const sourceMaps = require("gulp-sourcemaps");
var concat = require('gulp-concat');
const rev = require('gulp-rev');
//var babel = require('gulp-babel');
//const rename = require("gulp-rename");

var paths = {
    webRoot: "./wwwroot/",
    nodeModules: "./node_modules/"
};

paths.jsSource = paths.webRoot + "src/js/";
paths.jsDestination = paths.webRoot + "dist/js/";

gulp.task("vendors:js", function (done) {
    browserify([paths.jsSource + "vendor.js"])
        .transform(babelify, { presets: ["@babel/preset-env"] })
        .bundle()
        .pipe(source("vendor.min.js"))
        .pipe(buffer())
        .pipe(uglify())
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

gulp.task("globals:js", function (done) {
    browserify({ entries: [paths.jsSource + "globalsLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("globals.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
       // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});


paths.jsSharedLoader = paths.jsSource + "shared/";
gulp.task("shared:js", function (done) {
    browserify({entries:[paths.jsSharedLoader + "sharedLoader.js"],debug:true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"]})
        .bundle()
        .pipe(source("shared.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsPatentLoader = paths.jsSource + "patent/";
gulp.task("patent:js", function (done) {
    browserify({ entries: [paths.jsPatentLoader + "patentLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("patent.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsTrademarkLoader = paths.jsSource + "trademark/";
gulp.task("trademark:js", function (done) {
    browserify({ entries: [paths.jsTrademarkLoader + "tmkLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("trademark.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsGMLoader = paths.jsSource + "gm/";
gulp.task("gm:js", function (done) {
    browserify({ entries: [paths.jsGMLoader + "gmLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("gm.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsDMSLoader = paths.jsSource + "dms/";
gulp.task("dms:js", function (done) {
    browserify({ entries: [paths.jsDMSLoader + "dmsLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("dms.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsAMSLoader = paths.jsSource + "ams/";
gulp.task("ams:js", function (done) {
    browserify({ entries: [paths.jsAMSLoader + "amsLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("ams.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsClearanceLoader = paths.jsSource + "clearance/";
gulp.task("clearance:js", function (done) {
    browserify({ entries: [paths.jsClearanceLoader + "tmcLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("clearance.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsRMSLoader = paths.jsSource + "rms/";
gulp.task("rms:js", function (done) {
    browserify({ entries: [paths.jsRMSLoader + "rmsLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("rms.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsFFLoader = paths.jsSource + "ff/";
gulp.task("ff:js", function (done) {
    browserify({ entries: [paths.jsFFLoader + "ffLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("ff.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsPACLoader = paths.jsSource + "pac/";
gulp.task("pac:js", function (done) {
    browserify({ entries: [paths.jsPACLoader + "pacLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("pac.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

paths.jsAdminLoader = paths.jsSource + "admin/";
gulp.task("admin:js", function (done) {
    browserify({ entries: [paths.jsAdminLoader + "adminLoader.js"], debug: true })
        .transform(babelify, { presets: ["@babel/preset-env"], plugins: ["@babel/plugin-proposal-class-properties"] })
        .bundle()
        .pipe(source("admin.min.js"))
        .pipe(buffer())
        .pipe(sourceMaps.init({ loadMaps: true }))
        // .pipe(rev())
        .pipe(uglify())
        .pipe(sourceMaps.write("./"))
        .pipe(gulp.dest("./wwwroot/dist/js"));
    done();
});

//gulp.task("watcher-globals:js", function () {
//    gulp.watch(paths.jsSource + "plugins/*.js", gulp.series("globals:js"));
//});

//gulp.task("watcher-shared:js", function () {
//    gulp.watch(paths.jsSharedLoader + "*.js", gulp.series("shared:js"));
//});

gulp.task("watcher:js", function () {
    gulp.watch(paths.jsSource + "plugins/*.js", gulp.series("globals:js"));
    gulp.watch(paths.jsSharedLoader + "*.js", gulp.series("shared:js"));
    gulp.watch(paths.jsPatentLoader + "*.js", gulp.series("patent:js"));
    gulp.watch(paths.jsTrademarkLoader + "*.js", gulp.series("trademark:js"));
    gulp.watch(paths.jsGMLoader + "*.js", gulp.series("gm:js"));
    gulp.watch(paths.jsDMSLoader + "*.js", gulp.series("dms:js"));
    gulp.watch(paths.jsAMSLoader + "*.js", gulp.series("ams:js"));
    gulp.watch(paths.jsClearanceLoader + "*.js", gulp.series("clearance:js"));
    gulp.watch(paths.jsRMSLoader + "*.js", gulp.series("rms:js"));
    gulp.watch(paths.jsPACLoader + "*.js", gulp.series("pac:js"));
    gulp.watch(paths.jsAdminLoader + "*.js", gulp.series("admin:js"));
});

paths.cssSource = paths.webRoot + "src/css/";
paths.cssDestination = paths.webRoot + "dist/css/";

paths.cssSourceVendors = [
    paths.nodeModules + "bootstrap/dist/css/bootstrap.min.css",
    paths.webRoot + "lib/kendo-ui/styles/kendo.common-office365.min.css",
    paths.webRoot + "lib/kendo-ui/styles/kendo.office365.min.css",
    paths.webRoot + "lib/bootstrap-star-rating/css/star-rating.min.css"
];
                        // paths.webRoot + "lib/fontawesome/css/fontawesome-all.min.css"];

gulp.task("vendors:css", function () {
    return gulp.src(paths.cssSourceVendors)
        .pipe(concat("vendors.min.css"))
        .pipe(gulp.dest(paths.cssDestination));
});

//gulp.task("min-vendor:css", function () {
//    return gulp.src(paths.bootstrapCss)
//        .pipe(concat(paths.vendorCssFileName))
//        .pipe(cssmin())
//        .pipe(gulp.dest(paths.destinationCssFolder));
//});