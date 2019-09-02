import { Component, Input, Output, AfterViewInit, OnInit, EventEmitter, Injector } from '@angular/core';
import { HttpEventType, HttpClient } from '@angular/common/http';
import { AppComponentBase } from '@shared/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppConsts } from '@shared/AppConsts';

declare var moment: any;

@Component({
    selector: "upload-component",
    templateUrl: './upload.component.html',
    styleUrls:['./upload.component.less'],
    animations: [appModuleAnimation()]
})

export class UploadComponent extends AppComponentBase implements OnInit {
    public progress: number;
    public mess: string;
    @Output() public onUploadFinished = new EventEmitter();

    constructor(injector: Injector, private http: HttpClient) {
        super(injector);
    }

    ngOnInit() {
    }

    public uploadFile = (files) => {
        if (files.length === 0) {
            return;
        }

        let fileToUpload = <File>files[0];
        var filesize = Number.parseInt(((fileToUpload.size / 1024) / 1024).toFixed(4)); // MB
        if (filesize > 1) {
            this.mess = 'اندازه فایل بیشتر از 1 مگابایت مجاز نیست';
            return;
        }
        const formData = new FormData();
        formData.append('file', fileToUpload, fileToUpload.name);
        AppConsts.appBaseUrl
        this.http.post(AppConsts.remoteServiceBaseUrl + '/files/upload', formData, { reportProgress: true, observe: 'events' })
            .subscribe(event => {
                if (event.type === HttpEventType.UploadProgress)
                    this.progress = Math.round(100 * event.loaded / event.total);
                else if (event.type === HttpEventType.Response) {
                    this.mess = 'بارگزاری موفق';
                    this.onUploadFinished.emit(event.body);
                }
            });
    }



}