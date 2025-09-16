<template>
  <div class="code-editor-container">
    <!-- <div class="editor-header">
      <label for="language-select">Lenguaje:</label>
      <select id="language-select" v-model="selectedLanguage" @change="changeLanguage">
        <option value="csharp">C#</option>
        <option value="sql">SQL</option>
        <option value="javascript">JavaScript</option>
      </select>
    </div> -->
    <div ref="editorElement" class="code-editor"></div>
  </div>
</template>

<script>
import { ref, onMounted, nextTick, watch, computed } from 'vue'

// Importaciones de CodeMirror
import { EditorView, basicSetup } from 'codemirror'
import { EditorState, Compartment } from '@codemirror/state'
import { autocompletion, completionKeymap } from '@codemirror/autocomplete'
import { keymap } from '@codemirror/view'
import { javascript } from '@codemirror/lang-javascript'
import { sql } from '@codemirror/lang-sql'

export default {
  name: 'CodeMirrorEditor',
  props: {
    initialCode: {
      type: String,
      default: ''
    },
    initialLanguage: {
      type: String,
      default: 'csharp'
    },
    codeFunctions: {
      type: Array,
      default: () => []
    },
    theme: {
      type: Object,
      default: null
    }
  },
  emits: ['update:code', 'language-changed'],
  setup(props, { emit }) {
    const editorElement = ref(null)
    const selectedLanguage = ref(props.initialLanguage)
    const editorView = ref(null)
    const languageCompartment = new Compartment()
    const themeCompartment = new Compartment()
    
    // Datos de autocompletado para diferentes lenguajes
    const completionData = {
      csharp: props.codeFunctions,
      // csharp: [
      //   // Palabras clave de C#
      //   'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch', 'char', 'checked',
      //   'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do', 'double', 'else',
      //   'enum', 'event', 'explicit', 'extern', 'false', 'finally', 'fixed', 'float', 'for',
      //   'foreach', 'goto', 'if', 'implicit', 'in', 'int', 'interface', 'internal', 'is',
      //   'lock', 'long', 'namespace', 'new', 'null', 'object', 'operator', 'out', 'override',
      //   'params', 'private', 'protected', 'public', 'readonly', 'ref', 'return', 'sbyte',
      //   'sealed', 'short', 'sizeof', 'stackalloc', 'static', 'string', 'struct', 'switch',
      //   'this', 'throw', 'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe',
      //   'ushort', 'using', 'virtual', 'void', 'volatile', 'while',
        
      //   // Métodos y clases comunes
      //   'Console.WriteLine', 'Console.ReadLine', 'String.Format', 'Convert.ToInt32',
      //   'List<T>', 'Dictionary<TKey, TValue>', 'Array', 'DateTime', 'TimeSpan', 'StringBuilder',
      //   'Task', 'async', 'await', 'var', 'linq', 'ToString', 'Parse', 'TryParse'
      // ],
      
      sql: [
        // Tablas de ejemplo del esquema de base de datos
        'Users', 'Products', 'Orders', 'OrderDetails', 'Categories', 'Suppliers',
        'Customers', 'Employees', 'Departments', 'Inventory',
        
        // Columnas comunes
        'Id', 'Name', 'Email', 'Password', 'CreatedDate', 'UpdatedDate', 'IsActive',
        'Price', 'Quantity', 'Description', 'CategoryId', 'UserId', 'CustomerId',
        'FirstName', 'LastName', 'Address', 'City', 'Country', 'Phone',
        
        // Palabras clave SQL
        'SELECT', 'INSERT', 'UPDATE', 'DELETE', 'FROM', 'WHERE', 'JOIN', 'INNER JOIN',
        'LEFT JOIN', 'RIGHT JOIN', 'FULL JOIN', 'ON', 'GROUP BY', 'HAVING', 'ORDER BY',
        'ASC', 'DESC', 'DISTINCT', 'TOP', 'LIMIT', 'OFFSET', 'UNION', 'INTERSECT',
        'EXCEPT', 'CREATE', 'ALTER', 'DROP', 'TABLE', 'INDEX', 'VIEW', 'PROCEDURE',
        'FUNCTION', 'TRIGGER', 'DATABASE', 'SCHEMA', 'PRIMARY KEY', 'FOREIGN KEY',
        'NOT NULL', 'DEFAULT', 'CHECK', 'UNIQUE', 'AUTO_INCREMENT', 'IDENTITY'
      ],
      
      javascript: [
        // Palabras clave de JavaScript
        'var', 'let', 'const', 'function', 'return', 'if', 'else', 'for', 'while', 'do',
        'switch', 'case', 'break', 'continue', 'try', 'catch', 'finally', 'throw',
        'new', 'this', 'typeof', 'instanceof', 'in', 'delete', 'void', 'true', 'false',
        'null', 'undefined', 'NaN', 'Infinity',
        
        // Métodos y objetos comunes
        'console.log', 'document.getElementById', 'document.querySelector',
        'addEventListener', 'setTimeout', 'setInterval', 'clearTimeout', 'clearInterval',
        'JSON.stringify', 'JSON.parse', 'Array.prototype.map', 'Array.prototype.filter',
        'Array.prototype.reduce', 'Array.prototype.forEach', 'Object.keys', 'Object.values',
        'Promise', 'async', 'await', 'fetch', 'localStorage', 'sessionStorage'
      ]
    }

    // Función para crear autocompletado personalizado
    const createCustomCompletion = (language) => {
      return autocompletion({
        override: [
          (context) => {
            const word = context.matchBefore(/\w*/)
            if (!word) return null
            
            const options = completionData[language] || []
            const completions = options
              .filter(option => option.toLowerCase().includes(word.text.toLowerCase()))
              .map(option => ({
                label: option,
                type: getCompletionType(option, language)
              }))
            
            if (completions.length === 0) return null
            
            return {
              from: word.from,
              options: completions
            }
          }
        ]
      })
    }
    
    // Función para determinar el tipo de autocompletado
    const getCompletionType = (option, language) => {
      if (language === 'csharp') {
        if (['class', 'interface', 'struct', 'enum', 'namespace'].includes(option)) return 'keyword'
        if (option.includes('.')) return 'method'
        if (option.includes('<') && option.includes('>')) return 'type'
        return 'keyword'
      }
      
      if (language === 'sql') {
        if (['SELECT', 'INSERT', 'UPDATE', 'DELETE', 'CREATE', 'ALTER', 'DROP'].includes(option)) return 'keyword'
        if (['Users', 'Products', 'Orders', 'Categories'].includes(option)) return 'type'
        return 'property'
      }
      
      if (language === 'javascript') {
        if (option.includes('.')) return 'method'
        if (['function', 'var', 'let', 'const'].includes(option)) return 'keyword'
        return 'variable'
      }
      
      return 'text'
    }

    // Función para obtener el modo de lenguaje
    const getLanguageMode = (language) => {
      switch (language) {
        case 'javascript':
          return javascript()
        case 'sql':
          return sql()
        case 'csharp':
          // Para C#, usamos un modo básico ya que no hay soporte nativo
          return javascript() // Fallback temporal
        default:
          return javascript()
      }
    }

    // Función para cambiar el lenguaje
    // const changeLanguage = async () => {
    //   if (editorView.value) {
    //     const newLanguage = getLanguageMode(selectedLanguage.value)
    //     const newCompletion = createCustomCompletion(selectedLanguage.value)
        
    //     editorView.value.dispatch({
    //       effects: languageCompartment.reconfigure([newLanguage, newCompletion])
    //     })
        
    //     emit('language-changed', selectedLanguage.value)
    //   }
    // }

    // Inicializar el editor
    const initializeEditor = async () => {
      await nextTick()
      
      if (!editorElement.value) return

      const initialLanguage = getLanguageMode(selectedLanguage.value)
      const initialCompletion = createCustomCompletion(selectedLanguage.value)

      const themeExtensions = props.theme ? [props.theme] : []

      const state = EditorState.create({
        doc: props.initialCode,
        extensions: [
          basicSetup,
          languageCompartment.of([initialLanguage, initialCompletion]),
          themeCompartment.of(themeExtensions),
          keymap.of([...completionKeymap]),
          EditorView.updateListener.of((update) => {
            if (update.docChanged) {
              emit('update:code', update.state.doc.toString())
            }
          }),
          EditorView.theme({
            '&': {
              fontSize: '14px'
            },
            '.cm-completionLabel': {
              fontFamily: 'Monaco, Consolas, "Courier New", monospace'
            }
          })
        ]
      })

      editorView.value = new EditorView({
        state,
        parent: editorElement.value
      })
    }

    // Función para obtener el código actual
    const getCode = () => {
      return editorView.value ? editorView.value.state.doc.toString() : ''
    }

    // Función para establecer código
    const setCode = (code) => {
      if (editorView.value) {
        editorView.value.dispatch({
          changes: {
            from: 0,
            to: editorView.value.state.doc.length,
            insert: code
          }
        })
      }
    }

    // Watch for theme changes
    watch(() => props.theme, (newTheme) => {
      if (editorView.value) {
        const themeExtensions = newTheme ? [newTheme] : []
        editorView.value.dispatch({
          effects: themeCompartment.reconfigure(themeExtensions)
        })
      }
    })

    onMounted(() => {
      initializeEditor()
    })
// changeLanguage,
    return {
      editorElement,
      selectedLanguage,
      getCode,
      setCode
    }
  }
}
</script>

<style scoped>
.code-editor-container {
  width: 100%;
  border: 1px solid #ddd;
  border-radius: 8px;
  overflow: hidden;
  background: #ffffff;
}

.editor-header {
  background: #f8f9fa;
  padding: 10px 15px;
  border-bottom: 1px solid #ddd;
  display: flex;
  align-items: center;
  gap: 10px;
}

.editor-header label {
  font-weight: 600;
  color: #333;
  margin: 0;
}

.editor-header select {
  padding: 5px 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: white;
  font-size: 14px;
}

.code-editor {
  min-height: 300px;
}

/* Estilos para el autocompletado */
:deep(.cm-tooltip-autocomplete) {
  border: 1px solid #ccc;
  border-radius: 4px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

:deep(.cm-completionIcon) {
  margin-right: 8px;
}

:deep(.cm-completionIcon-keyword) {
  color: #0066cc;
}

:deep(.cm-completionIcon-method) {
  color: #7c3aed;
}

:deep(.cm-completionIcon-type) {
  color: #059669;
}

:deep(.cm-completionIcon-property) {
  color: #dc2626;
}
</style>