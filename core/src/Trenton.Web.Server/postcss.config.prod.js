const tailwindConfig = require('./tailwind.config')
tailwindConfig.purge = ['Views/**/*.cshtml']

module.exports = {
    plugins: [
        require('tailwindcss')(tailwindConfig),
        require('autoprefixer'),
        require('cssnano')({
            preset: 'default'
        })
    ],
}
