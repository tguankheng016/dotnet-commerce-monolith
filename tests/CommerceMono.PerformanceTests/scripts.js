import http from "k6/http";
import { group, check, sleep } from "k6";

export const options = {
    vus: 10,
    duration: '10s',
    thresholds: {
        http_req_failed: ['rate<0.01'], // http errors should be less than 1%
        http_req_duration: ['p(95)<100'], // 95% of requests should be below 200ms
    },
};

export default function () {
    const res = http.get('http://localhost:5295/api/v1/identities/current-session');
    check(res, { 'status was 200': (r) => r.status == 200 });
    sleep(1);
}