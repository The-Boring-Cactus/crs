import { App } from 'vue'
import { createI18n } from 'vue-i18n'
import { message  } from '../locales/en'
// const localPathPrefix = '../locales/'

// import i18n resources
// https://vitejs.dev/guide/features.html#glob-import
// const messages = Object.fromEntries(
//   Object.entries(import.meta.globEager('../locales/*.ts')).map(
//     ([key, value]) => {
      
//       return [key.slice(localPathPrefix.length,-5), value]
//     }
//   )
// )
// console.log(messages)

const messages = {
  en: message
}

const install = (app: App) => {
  const i18n = createI18n({
    legacy: false,
    locale: 'en',
    globalInjection: true,
    messages,
  })

  app.use(i18n)
}

export default install