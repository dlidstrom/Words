import _ from 'lodash';
import printMe from './print.js';
import './style.css';

function component() {
    const element = document.createElement('div');
    const btn = document.createElement('button');
    btn.classList.add('bg-blue-500');
    btn.classList.add('hover:bg-blue-600');
    btn.classList.add('text-white');
    btn.classList.add('font-bold');
    btn.classList.add('py-2');
    btn.classList.add('px-4');
    btn.classList.add('rounded');

    element.innerHTML = _.join(['Hello', 'webpack!!'], ' ');
    btn.innerHTML = 'Click me and check the console!';
    btn.onclick = printMe;
    element.appendChild(btn);

    return element;
}

document.body.appendChild(component());
