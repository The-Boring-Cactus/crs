<script setup>
  import { nextTick, ref, computed } from 'vue'
import { useLayout } from '@/layout/composables/layout';
import AppConfigurator from './AppConfigurator.vue';
import {userStoreMe} from "@/store/userStore";
import { useRouter } from 'vue-router'

const router = useRouter()
const userStore = userStoreMe();

const { toggleMenu, toggleDarkMode, isDarkTheme } = useLayout();

const selectedCategory = ref(null)
const exit = async () => {
        userStore.setCurr(false,'','');
        await nextTick();
        await  router.push({ path: '/auth/login', replace: true })
        console.log("Save");
      }



</script>

<template>
    <div class="layout-topbar">
        <div class="layout-topbar-logo-container">
            <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
                <i class="pi pi-bars"></i>
            </button>
            <router-link to="/" class="layout-topbar-logo">
                
                <img style="width: 30px;" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAFwAAABbCAIAAABxiHW3AAAAAXNSR0IB2cksfwAAAARnQU1BAACxjwv8YQUAAAAgY0hSTQAAeiYAAICEAAD6AAAAgOgAAHUwAADqYAAAOpgAABdwnLpRPAAAAAlwSFlzAAALEwAACxMBAJqcGAAAIABJREFUeNrtfHmcFNXV9jm3qreZ7pmefQdmGBh2ZN8RkE30dSFEMG6JGjWrMRFjYl6jiUs0aDRRo4kS3xAUDApGUUGQfR8YGLbZmIXZ957el6p7vj+qurt6pnuYUZN/vtSPn/Z037p177nnnvOc55xbSETw3yvyYv8VwX9UKIoOfl2aSJrra+mqnwb43+3z3+3z/71QCEAm+b9CCV+c+KXOytMtJV/iXjG2lPGrLRJe0QYjIgG5vG67yyFJgXiTyWqxCigOoHOI3T8BQLe3+2xraae/wywkEBAOci6xRoBfh/L27ZAU005E7oDrWGnxJ4c+/eT87mpvGwewMuOCvGkr5960aMbVadZUBIaIff0GIPY7Prxsqy9pPxngfgHBK7k5cQHZoGb0dXkfAgCJS+3d7Y2tjQ1tDS3tzZ2Obq/fJ8uy8ghBEJISku5b+V2zyXyhtuw365/+sGZ/1L4K4zKfvf3xa+csYyhs+eL9C5fOy5wTEQExhnpBb4mzZKZkZqdm52TmZKVlGkUjYliCRNTibD7cdJCAuCzfOHKlQTQOajIDEgoR9Vm0sCz8kq/ycuXh0mMfHd2+t6nETzxqJ1bBuOt/t44tGHOy7NTqdXc3+ruDXZAeRT0KDu5jwfVEgD+tfOI7N95ld9u/9eRde1pOx9LnkfGZ35xxw6IpCyeMmGA2mQHAL/uP1R/p9HbkJxZUdFctH77cYkgY1AS/ilDI4bbvP3lg/Wd///jy0Stq57rrH/nBLT+oaapd8cSqWm87ABhQeGThfSvmLMtJzxMEoaOr/fCZw7//+JVqT7tyywf3v3Ht3GUHSvYtffGuKw5yvGXID5bds3jOojp3fYuzpShl1Jj0ojZHu9mQYDaYvx5Nia0dAAC+gHfPiT3PvfeHI51l2u/zDNalRfMn5E/ISsu0Wqw6QWSigACIODx3uCU+4cEXHl5/disAFJoy3nrw1WmjJymaTwAInIhfbmta+8qv/lW7HwAK4zJ2PbvNarGWVp5HQMVeSrLkcrtauloqGir2Xzh8qKOMaexFsmi6YdqilQtuuGb0YoYiAsaaSMjYD1QooS+jmrrqpurnN7zwf+e3h76dbC24c8GameOnF+QVmI0JDFmfrggQTl0smf3MzQggAO58eNOcibMUMCGT3O22pcSlAgAia2hvWvGrlRXuZgB4Y81Td6y4DYGp3irktogIuE/ytXS0nCk7s3nP+1s1FmpWStEzd/9mxvjpCAxx8K6DBnFxSQ5sP/hxwd3jDLfnKf+uW3vDjsOfOTx2tQXxqDdyLnPyP79hnXLX2pcflbms3kGS02fbeGyTxKVQ+42fvqu0vPmXt/gkL+e9uuUU+SBvwPPOgX8senRpaGDxdwx9ZdMrTo+zz71Xvgbufcgb8Pzlgzd//vE65YZcvfXp1Y9df/V18Yb4AawDBWT/Db+4ZU9zCQB8/tN35k2aG9IkAkIADOsXVTdXj3tkEQcSEOteOZmakNrvuvIme+Ohy4eMgsHiSXj+3Zf2tJ5Rfrp7/P/85v5fpyamf+2IlgDI7Xf/7u3fPxKUyO2jl3/+1Ierl94yMIkAAPol+Wx7hfJHbmZ26HtEZIAaiQAAZKRkpIhxACATOZzO/ru2+xwnm08aRcP8/AULpyzc9OSGZ5b9RBnT+rMfff/3D7Z0NX39MN8n+db9/cXf7X9LkdDTSx98+acv5mcNA8CB71Ui8nFJ2dx6nT4C3fW2XCgy0ayLUwE75/1065W8JxqOE8H0nJlWUxIAS4xP/PGtP9x8/x+TBCMA/Kvu0IMvPtxua/86hSKT/NbW9c/s+ysAMMA31zz94G0/jjfGDxrdacA2AusDeftolhy4IrqWuFTceMIRsI9LH5+ZkBV6lsDE6+dc/8HDb+carADwYd3Bx19/wuV1fnWhEAAB8F1HP//Zv36njO2NNb/91opbRaYblI6oXWmtF8KVhHLlBhz46eaSFmdjfmJBQUqBVqCIiMBmjp35zk/fTBbjAOBv5z/66/tvDjBo7l9TqKal9ifrH1NG95slP7p1+a0DCNiig57BTJi0jF100ABU2V5RbatKj8sclzk+MuRTgBFDZNNGT33ruy8KgADwi89ePHT60AAWI6ZQCAD8cmDdhj/UeDsA4JuFC793ywOioPtScREqHYYHTjgQXYghRAKAZnvzubbSRF3i9LwZIhNibkLEJTOXPLXiIeW2X/7tiU5751fSlEMlh94q/VCBiY/f/ZjZaIYvdWGUySu7LxZupH5FRjaP7WRTsVEwzRoyx9hvsEcAAmP33Hj3ityZAFDcXfXezn/SlRYjplDcfvcL7/1R+fzUTT8vzCv8CjG0CoxpANsnSAwgxdhWnoD3eMNxTnx67owEYyIAxkZahEAIaDFZfnHHWqXV09tfbmyr/xJC4QB0rPTYruZTADDWknPTohswmmUN0eL9IkAK/oc0ssDYphqJonbIFQR4ouGEK+CcmDEx1ZzWTywCAERAhEQIiJNGTXpg0koA6JDcnxz4VFGWWMNm0RQYJC5t3LlJ+fOh67+fbEmKpQBKGNJP6KjOHHtZBbqSakWh2mSSTjYWt7qaRySNGJo8LBafRgTdXlujo6nJ1dzsbG52Nrc4mnu8tjtWrFE6enXnW3Z3D0HMYbOoZrGuuW5T2U4ASBHjrplxTaxdFtYUIE6cE+dAPHLCmhwL9sPvhePG3pYl7MDL2yvr7HWZ5uzRmaNiS4QAwCN5bL6eHm+Pzdtj8/V0+2wOv2vc8PHXD50JAOXuljNlZ4BiaooYdWVPXiiRiAPAXVO/kZmSGVXPFWaQc/lM1bkvjn9RUnla4vK4YWOvmb5g2pipOkHXV4MotklRI2BECEq510ObehrLO8qSjSnTcqaxGCyqVgGRgn9ytSeDaLpt4Zrtbx8FgD2n9s6bPA+BRVVzMRoPTvtOq2H4NVMXYlRtAkBEl8/9p3dfe2L3H0M/fFi7/6m9f147++5Hvv3TBJNFu+HxSjgFETucHUdqjs4rnCMKTNu+2919sqnYIBhnD5ljEI2xNisqolCQYugRGPwNYfKYSSIyifi2k5+uvePhOEN81B3E+q6/0+PcUb4fAPTIigpG9sPIvrDhJa1EQmNYd3j9ur+/JHH5Cp66z5VoSpiYPd7pcVU0X/KTpHzZ47GdaDiKADPzZsXrlWlg2H7EEjOG/wW1DrPTs2aljQKAc87GxrbGgXofRGjraq/zdQPAeOuwtOTUPo9THsFPnC9+dt/rylf3TVz18Y/+9smP/++BSatV8vHgW2fKSzXhAtIAUKxO0A9JGZqbnDs6e4wuCJ2P1h9xBVxTc6YlxyX1Wj8JuNPnsjltHsnHIcykBdMavZSUdEw3b8wcABAA61saBpHiaO1oUXDxzMLJekEfbYWJA3x4YLsyy5/MuPPJ+//XoNMRwayJM+hV+Y1TWzjQ58d3Tx51lYYTwEgtIY3ZULGGSksCMCV8UWJ02ZdqTtfGewAYkAOHTh9574v3P764yyG5R1jy1sz+xs0LbxyWPZQUC4Wq1UXSODDEorwRSi+NrQ2x8lNR3EqPs0f5MCwjP1bUJ3F574UDyufVi1cZdAalpVGnWzn/RuX7Y+UnKEbgx4lfaLxIqljQ4XGca7ygeANtzBMSoMvvBJABVA7B5XM9+denl790+/rSrW0Bh4fkUnvtLz97Yf5jK3Yc201AIeQX/hCcSFqSylc1d7XEwpB9hcJcHrealDBbMTrEQs7J4XcpihoXZ1JEjoiIoskQFyS3/SEP2yv2ISC71xHaUC32FpnL2iF6Ah4pmCohoExzVqgTTvTqptfXHVrf15q0S87bX3vgXNU5DYLrvUnjTOrwelz2WEvOopIUygedKPZHARL1MXYR8DzSsEf8xECYXjCNAQMAiQc8kvuqIRMYMgDyyp7TzSWfVGz3ksqnFCQV5FpzuCojubyu4re7/6T8dNuoa/c/uuX8s19suf/1ovhsAHBy/2vv/yXAJeBBeBMpmlBM248fEPt1Dyy4+7XZgBCOoPC6q8lMAkAgiuFoSGvOmdqQV7SUF6YVAoBMvLar5kTzcWfAadUliUFjZDLEt7g6E/SJggAEdOJ8cYA4AMxNG7/uwWeTzFYiLMjOt1qs16xbjYCbLmy/s/M2qyXxii4vVsZ7QOSIatKJ7L6esy2lAR7ItxaEAEDoV8VSUnDng4pKUYsjeg3O7rNzlE16Y7uj9VTTyTZvh0E0zMu7Oichy4BPAniUxoKaV0YAaGhTCddVs25KMicpj0PEq0ZNyDOmNHi7fCTbHLYki5WINKF4X5cdMzYRo7nG6BlSDtKBy/u7fTYRsNnR7CVf5FSJiAOxyFXBYF4tAnGqpgp5eWv50JTcvdVfNNibjIJxSsbkwrQRItP7JU8kQ4WghHYAPKj2JmMcKBkyBAAUGIsXTcGxcoKg60GMFZ/HyoeJA+YBkYA7/HYCmQhlAjmoETVd1QlJZlEUdl3YfcvkVTxM+amON5jECrNvRBwAul3dTc7GC10XZC6NThk9MWuSSRcf3IYRDpyQQsA/NAdOUhjDAxKxkNC5EiCr2kBRQwHOZUCIGkNFjX0wKt8VkGQd0/mlgJ+4SYgTgka6vOtit9BOQKSnMy1n2lztmts5IkI4Tadefu6v6qg623rG6XflmHMm5kxMMaUxLaLR3kCkkHdB9jtYXQCcSHb6vQZR327vaOpu9nF/hL5HAwShjgUmDGj7KHpO0UQSkKWj9UcCXJqcMdmiT7AYzHp8TvlpWtaM5NSkVldro6PhdOvpqoaa4MwDDr/dojcDICe5290dCoR3VH2uM2GqMe2agsVZCVna6pWQg9IMIyJkDjWx+5wXW8u3nN2yJH9JUeaocXnjEnUWgA515hhCmkHyIvJ+HKChRYxoScABOACTST7RcLzD0zkpfUpR2kgA8ATcoRVLjkspSh85Ekb6ZX+Pp2e/8yDABgDo9Hd9VL4tQW/NtuTafT3trg4puOOcXrsBhCXj18TpLL2WJKTbmogOMRr7IMvSsJS8tQseMQgGROaX/DzcjAEQKghZ/W+ftY5d39RXUwgjQnySKXCq4VSTs74oaXRRWpGy57VRmUxKWQ4aBGO62ZhrzVW+N4vmAuuIFldLZXe5gelIU7eSZc7ITcnRMX0UzhIgIPu3n/vIzj3RKKnwx5T4ZJNoIhIAkIhz4FIwhgxGW8B6aUlvD4gDxSkaUggB4ExzaWV3xfDEgnFZ44OqBAgMI0TJQzgwBFPMYvyMvOkA0ObsON54iCBAQQu6oHBBVnJO1KWSSPr04qdnu8722S8K/AsXsDEUAUQCKm+90O3qKkgfEcqfUZAXDzogjWBQK+roG0jsx/cgsEud1eWdF9Pi0ibnTglaywh4EgSNCERuvyveYO4VDQsoZlqyx6SNL205GzYYKDKMvlDFNSdONB/nPEzgESdNDIUUmd1pszdvObc5QIG42vguv703Wsc+8+43nRRVKBGcYK2tWt/Gko0pc4fO1TFDELaq3kHjBWSHz1HVWrW3Zs/3Zv+Ayzy0rbyyX0QOgDmJuXF6swgMQI7h9AkAZS439jToQPBp88caAIaRYCcgB3aUbUfkImCP19YteTAysRTM1kahXfqhMsS+tECY7/J2mwTT3CHzTKK5HyxT3XnJZDGMzBiZZk4vrT/n8DrUgJD7m9ytOhAFJgCSR/L2dQEhOpaA2z2OBFNiliXrYufZ3j4cQwxDGOxIJBXXHW90XEYgAVFkrI+WhJ0XRAm+MRZYFfsJEHSon5E322ywREtZhPm+UWmjClIKiMiUmJmakPrepS2qvZQkqy5Rz/QMEABF7oqxONRsb+xwd/m5/1TpiWk5M/TMFGDu6CuFYXtc1Vl1uboCgCuISdAWcyi1lAQ8zB1gbwVBGhDzpiSiQvtiTPpYqzGpr0Q4yRJJmj0pADKbp6eyvcLtd47PH6vYg0s9DeDHREOCxZBgMZjNuniKkgYERPRIrjHpY7PMGY3O+lONJ64tui7XnKvRFdJEDeHhlTSd8smBgMzvnHLvDaNWMRT6Joo0cSv18ch9a9D62T4YkbyP5LvJLXlanK3egDdAarHJ6Ybik+0HG90NPu5L1KcsLbw235hW5+2wyZ6SCyVLZy0G4ACcU4DCA6QQTSkT9/oCHc62spYyTrzH31WUOUJkIsBzmmlEieqUmchcJgCX36lVQ1L3jBblI0SzFBgtnSZGy/xGmCWtRADI4XNw4lq6/FTzseQ0q0x84bAlZ5rPbDm3eUTekLrKDgBY98+XJ4+ZlJKQqAaEvfAGyYB4ofGC3du15dw7EnAAJMLDtYdMzByqeRSQBSOAEMcIKmVJMDJ1zJ6qL2psFRy4VuUjgh6KmWcbcNoUYwbXNq/DFfAgoMhEMaiuOtQXJI+cmj1zUu6kiZlX+bmvcNQQEREA9reXPvjS2vL6KpkoYgEIOAeX5D5ac8zHPQfqdjMGImMIUJg+8lDdvqrO8hS9Soikx6WLTEBAIuAkdTi6VBxkNhsEQ3Zi9sWOcwEuBWQ5Om+iYpZBFJb3RbQYFVMRkN1n7/b1iEzMiEtjTMi1ZNV5OwEgHpMabc1nfWdLW85MzZ4+IqGwTqheNnHq9tMnAOD9qt0f/WrPN0YuyUzO9AcDaKffcbC67NDlwwsKF31etcPH/SKiAGx46vA2R5tHCpR1Xmjz2ZTGcfr44dYCRAZAdo9z94V9yveW+PgZubPONp6ZN2SBwMS9Vbv6y56E0AReWWeixj6kuYkrZtrus9t8dj3TpcWl6gUdAUwaOuFQ+zkAOFZzYlxCkYEZgPOTjUdm5M71c9/ocT6X27234jwA+Im/W75Du3E2lrytNwnzh11TUn/CKTmUnRInGDISMo7UHkSggCyFtuehqoP5OXk6pgdiu47sLXM1AoCIrDCrEAEc/u55hVfrBOHApX0RWL43aUADTz+xK9XHcAJu89q6fN0iE9LjUvWCTnnqxOETlDaXm5qB09Lhyx+cv3Zm3ryDl/cMSx2RYsqYPG3c6vkLhph7J+cTRD0INCt7boejtdXZiEQAJBEfkTm6vKUMAZmq7OpwT7We2nrmQ5fPvfPYrh//45fKl3MLxk8aPqm46TBDRFKKK8MDZ4zFmNRAxBQtIOwVpXd7e2y+HoGJ6aY0ncr6EgCNHVGkNLrY3nSNyy+ioBP0AOjh/j21u+cNW1jafFqXLw4blt3R1t3Z2e3xBgBIEDErO310+lhAuNhRKgooceAACQarV/IlGVNXjl3NgW8ofju0gpVVNTW1Da9tXH+0rUz5cogx+e2HXm9yNLkkjx50CCCASP4wQDCZ4gBoIEoxkO0DkZw72nxOm88uoJgZl64T9EEvRgA0cljhlKQRJ7srCaimuh6nIgBJXJJkDogHavcuKVimQ/FA3S4xE1MzkmUihQdPNWZmWnP2VO8UEQCQMSROhRkjS+pPTkifmGXNAkSDJgm3r+Ksdk5GJt53/R0d7o7qjksykaTOHl12n9KgMD4rPSmdk6wtFULqDaaJaOBRMoUMdY/kcPidOqZPj0s1CIaIbDVQnMH0wLV3f/edXwDA3nMlD13nA1ILMhBQAsnjd80uWmISjc6Ao9XV3mJv0TFxXv58t+T68MIHHLkETEAQAfNTCjrtrTJJKr1AgIzlJaZUdLf2GlyeJene5bdPHT1x6/lNPi4BsADJMpcQoam+TWlz3cSlcXqTw+8Egj4IsBfkGXyULHFJRDHVlKxXJRJhjBjAdXNXTP7krVO2Kqfsf3/HtknDryKCiWlXLRuzrLaj7tPybZUdFzu8HQx0WZbskamFI9KKhibnV7ReRKak8YgAdYIhOzHncM1BAZABQ2SciBNfOH/6kMrLPr/f7fWOTB7R4m9KzUgeO2zczGFTD9TsZsBSdCk+7u/22zlQa1fbkWr1TNDCSfMZMOxVl4rRFx8HYFNC6RuVxUw1pcRIJwMAS7ZYn77r19e+fAcAvHZyc9bmrHFTRsXp4pNMKZ2GLpNompg12ebtOdx46LK9pslVd6a1ZMGwRedbSgVkivpKxAvTi6raywUEQobKuTYkArAmWmdMTxMYBWR/Uco44FjTUzN5yOTdNZ+LRJlxOffO+V6nq/PFfc9z4pt3/rNDcgHAaHPutPHTXLITg6wuROJGjEi5DLIQEAASREs0iUSgovmT5/522UPK37/a+dKrW94AWYXFIhjnDJ8/K3+WSMKqMavvnHiPTjBsr9xa57iEBAJDBB4nxkvA29xtAMTUGjAlmIOFQxc/PP/n9037gQBCVefFKUOm3jH1rkO1hxBFREEpkxWAGUXjqfLTT3z2sjKGR278iTU+MQKfRwI3rgHEg2DehGBZqkxytF0X8Q1Ddv/Ke5o6mv58cjMC7Kos0W0x5JqGGhN1Hu4jkBkyo6jPsealxCePz5rQ5my51F4pg58hMhTHZI0901gSkDkwZMppKAIgkkliiCadUZIDModsc7bP73P4HVcPmz+3YPax6mPnVMoKbT3On2z7hQILF2ROWjF/qSaAJMWsaP1QKG0kMGHgxDXEGdUUtNvj6psw1YatSj2b2Wh+/N5fGkT9S8c2AMCnl4/uePqmGZlF+UOHVI2oiY+PQ844534eON9wwe535KcXWQwmt8c5InVUeUeZgRmuG3kDAzxWf1ghBoiIA5dJVpRRJlg8cvmw1Px3T/59iDVfh3ok8MkBTnJt0+X3dnzW6XcDgFUwPv3dX5uNFuKkUoGkcQwqEAWvT83hmY3xoJyq6VPLKPZlZxPi1WOILd2tgByI1Xc19HhtoFpzRAKz0TIkeQgAIHIgsMYnPHn/4+Pyxz78zydsspcDHWkpO9JS9u6xnYmiMV5v2Pt5sdvvbXK3OiTP1MLR82ZNvnrYook5kyvbK6z6hMm5kxDwfMs5ZR6ITACBk5JLAM5lmUsIIIAgogCAnMjtdW/e9f6j7/6mM+BWsMkbd6+bWDgBg/Uo1Cv/o8qGd/Z0qCFVUnpova8YJUNacqoi4rO155VDvTVd1Rc7ziIgMuAAnPiI5JFDkoaoz2SEwAw6/W3X3Tp13KQ/f/DXN05tCQ3JJnltkrfRXRrqf0/ZmXGj8z+/9FlZe1mjoy7TlIUgKFQsJ1lJTSgBJ+cgk+zjAQ6cEycAb8Bf01R37OTJP3+xsd3rUjo0ovjsLY8un72Mhcjz6N6WCHhNS63yR2ZyZqzgJ4pQ0lMy4pjo4dKxhjNOjzsxLhFROW0OFCysQSV5QISAAVmu7qwkIBm4bAzcueZb02ZMaapu/vjEjuNd5VGf6pckPZeqbZcQUFYRPTIKkRuEgMUVpz7a+bnd4yjtLO264DDrzKcbShvtrZc93dqTlGMS8x697aHxI8YppJyaPEKKGhTKxI+XnVQklpuZNyBDq3SaZLEuypu2ve5Is99R39yQODwxyHcRJw6IDBlT4y0kAE7U7m7jaoIdCCgvK2fl1JseXPPjju6Orp4ul9dJAAFJuvkPd7l4QGGGgAiUY49qWI5abBGQpdc/erfJrdZUnW6s1fhLdSYJguHeq9csm78kyWLlSofhgndU4QVqnBBAV0/33svFAGAVDDkZWYOoTxFQXDpp0fa6IwBQUlYybvhYQAXqcK1hV82YgsFISaGpeRlGSAAGUZeTlp2dmqWYBrffK6IAEFD0WFBTR0EATRSscyMAkmSpK8aZJROKV+dNnT917uRxVyUlWhUfxLU1ogqFHzoSEcxmE8DF6ooe2QcA142YZ7VYB1e0M23sNNoGCPD+wa2rl9/CucyVEne1BFOxh0FrRmEogBFsY8ilMUWhScPL6gUDAkqcKwdUEUMNMBKew8OLvpOamJKbmGuOMycnWnPSc5ITk2vtdVwmQh6qRe7FIlGIOVDqHgCB4PNjKudy7fRlAgqDE0pRftG0pMLi7qpPG4rPV52TmRxRvRORv1YDrnAuoffYMEJWijKS/kfz1upRt79qX2VXeQhQBOtdIzi/q6fPy8vMG55UIKKoLL1P9smyHNQJCJe/aFFrKDlIai18c2fz347/EwDMTDdjwsx+UuwsKkEVp4+7Z8mdyhg37tgskaxWQ6BaZhL1zRQUTE9iOLmJMapluElnNOnjxHA9BMYoBwGZuEw8uCSIfYpKgmvU+8tggl19u8y/9n3UKbkB4O6pq3LTc/vJJbNYCH7xjGuSBBMAvHpi04ny40xl9nlQJNoNEhQJhnWH+sYWEYXSwYMOqp9Q6wJCAUsEcNBsUvUROBDCFcOqSVTfXv/s9j8pN69e/E2GbJDENQAQ5KTlPLL0+8qQPtq7U6VvkaF2UsGkUgg79n+Gvu9p01DNK/ZOG0amxKlXtIvRS62ipMOVQk6+bfeHrQEHAHx7/HUTiyb0TzixWCNHwNXLvplvSgWAkra60jPnMVTzF7kXqdeRpuinaHiEJUQMgh4K6X+wB1Ir7GKKNOSwIlnGXkyjWikHjPB8xdnXD29RmN3v3XyfyHSDTnGEJpWZnPn0rf+rfN54eEdrSxv2MfSqhvaKODGW/kVm5NRtoimOCGpgFJMdlkSU/qPnHwgYoM3Z/eLmV5Wk5eOLvz+ucNwVecn+thYSXjd3xY+m3QoAPi5v2L7V1mOn0DkaoKBzuSL52TdJRxS98oA0JjvC6IZJBdJ4fOztI4LnspTm3O13v/LO62d76gFgUebEe2++W5tdjfUWvCucYNeL+rV3/Ozq9AkAUOvq3vDhBz0OBwFy4EHAplnoGDw5RaPSUc3Khg9dElLUI0kUK0eD/SkLB+72e157940PKvYBQIbO/Pz3nkm2pPROciEOVChqUwQASEtK++ODLxSY0gCg3Na6cdvWrq7OoA3QpvIjbELEUgdtBmoMVtCLsj52tbdXJs6DWw0RQ/+LUuekgflkd/e89s5rG0s/AwA9Cm/e/9LYgrERN8TOn8bUlLAQCUbmjfj7T1/P1icCQEVP6+tbNnU0d4RqqCj4KWQrEDFek/nbAAADyklEQVRKgTNFIFqNwQ354iCqiEQimt5Ugx62s5oHatt3dLc/85fnN53bpbAKb97x3KLpiwb+YrP+t09Iv3BK0ZR3f/bXYcYUAGj3u3741uMbtm90+9yhrcCBOKkVrxTVZEQ7nB3C/poyrJAGUsTyIPVCmGrdiFq2rCqPzKU9xXvufe6Hu+tLAMCAwt/ufH7V4pUCCgN/P8NAXhWijnX66OnbHtu4IGMCALh44L7Nj93x5HeOlB6RZD8FC4CVl6FEvgIjiqHB0IvXIkpkox9cp2gmSy1KRyAADiQTl0kuq7u49uWfX/vHb593NgJAtj7h/R/+ZdVipXRlEK8fGuiBBSUGLBo6esOv1v9h48svHt0AANvrj3687ujNw+fNmj59ZEGh2RjPNLXgkYQuAUY1olwmrmwITjKpZeXESZu14pHHegkBBRQ4yZwAAPwB76W6qvXF698qft9PXLnt+iGzn7rv16OGjvwSL/Ib6CsuQi+JSk1Me+KBX8+fPPfxfzxdar+MANsuHdh26UCe0XrtpAVjh4/JyshISkxCBFJrDFjICGiqTGUCmUiWKRjWACIIMpcIJIj0QYTEiSvERbDemRDA4XLUN9eXXSr77MTuU101ofZWwfjbG9euWb7aYrIMNmE6OKFo5aJjumUzl08dO/WD3Vuf3/5Kg88GAPVe21+ObIMj23TIck1J8/KnZyVlJpjNOsHABHbPynsQWGhPVVbXbNm1TQDhXMu5VmfrFv8HCKy4rkQGDp06Ajh78WIgCGqHWPKGJg7514GPGpsbAMHhcnY7bcW1Jee76hTKKnQZUfjh7Nu/8z93FeQUYPgdE4PWlMG+JlF9+WPoUHV7T9u+4gMbd2/6tOFErIcPNaWUvHYIZCi8f1KX7BnsEM//bl9+9pAn3/ztc/vXx2pTGJd+/6K7ls9ZOjynkEU9yvlv0pRwNV4444FpiRmrrll1w4IbahprSsvPHr1wfG/5gbPORqaBW0w5xSQIE9NG7Rnku1BNTBdvikNkvTghDmRmuoW50xZOmDdl9JTRhaMS46zYu5btS15f6YWafe8lIK/f0+XotvV0O91Ob8AHAAITZ0+YxZhQebnyg93bnGGeMZhfUICJ+m6zEJwhQRAXTlkwb/IcBCypKOnq6ZJl2aDXxxvjEy2JKdYUS1yCyMRepUp9B4mI/zmhwEDf6609tEbYiyqInv6nXujzCq/qxi+jILHk9W958fd/6mXipD0i8DVe/b07ciDPG0jLEPGvhaQDl5s2rRn7pZbw1XfNv1dTvpxm/TvW/Mtd/w+3Jt8Edq/dCwAAAABJRU5ErkJggg==" />
                <span>ReportFlow</span>
            </router-link>
        </div>


        <div class="layout-topbar-actions">
            <button
                @click="toggleDarkMode"
                type="button"
                class="layout-topbar-action"
                :title="isDarkTheme ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
            >
                <i :class="isDarkTheme ? 'pi pi-sun' : 'pi pi-moon'"></i>
            </button>

            <div class="layout-config-menu">
                <div class="relative">
                    <button
                        v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
                        type="button"
                        class="layout-topbar-action layout-topbar-action-highlight"
                    >
                        <i class="pi pi-palette"></i>
                    </button>
                    <AppConfigurator />
                </div>
            </div>

            <button
                class="layout-topbar-menu-button layout-topbar-action"
                v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
            >
                <i class="pi pi-ellipsis-v"></i>
            </button>

            <div class="layout-topbar-menu hidden lg:block">
                <div class="layout-topbar-menu-content">
                   
                    <button  
                        type="button"
                        class="layout-topbar-action layout-topbar-action-highlight"
                 @click="exit">  
                        <i class="pi pi-times-circle"></i>
                        
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
