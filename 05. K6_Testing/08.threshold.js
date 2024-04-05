import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';

export const options = {
    ext: {
        loadimpact: {
            projectID: 3690093
        }
    },
    vus: 10,
    duration: '30s',
    thresholds:{
        'http_req_duration': ['p(95)<3500', 'p(90)<3000'],
        'http_req_failed': ['rate<0.01']
    }
}

export default function(){
    const response = http.get('https://test.k6.io');
    check(response, {
        'HTTP status is 200': (r) => r.status === 200,
        'Homepage has welcome message': (r) => r.body.includes('Welcome to the k6.io demo site!')
    });
    sleep(1);
}

