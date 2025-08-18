import Scanner from './escl-sdk-ts/escl/scanner.ts';

export enum Driver {
  Wia = 1,
  Twain = 2,
}

export declare type Device = {
  Driver: Driver;
  ID: string;
  Name: string;
}

export enum PaperSource {
  Auto,
  Flatbed,
  Feeder,
  Duplex
}

export enum BitDepth {
  Color,
  Grayscale,
  BlackAndWhite
}

export declare type SelectDeviceDTO = {
  device: Device,
  paperSource: PaperSource,
  bitDepth: BitDepth
  dpi: number
}

export default class WebScanner {
  deviceList: Device[];

  selectedDevice: Device;

  async scan() {
    console.log('Starting scan attempt');

    const ip = 'localhost';
    const port = 9880;
    const scanner = new Scanner({
      ip,
      port,
    });

    try {
      // Query for scanner capabilities
      const capabilities = await scanner.ScannerCapabilities();
      console.log('scanner capabilities', capabilities);

      // Start a scan job
      const jobUrl = await scanner.ScanJobs({
        // Set scanning options
        Resolution: 300,
      });
      console.log('job created', jobUrl);

      // Get the job ID
      const jobId = jobUrl.split('/')
        .pop();

      // Wait for the document to be scanned
      // If you're scanning from a feeder, call NextDocument multiple times for each page until it
      // produces a 404
      const doc = await scanner.NextDocument(jobId);
      console.log('scanned document', doc);

      // Turn the document JPEG into a blob and load it into the <img> element
      const blob = new Blob([doc.data], { type: 'image/jpeg' });
      const imageUrl = window.URL.createObjectURL(blob);
      const img = document.querySelector('#preview') as HTMLImageElement;
      img.src = imageUrl;
    } catch (err) {
      console.log('scanner err', err);
    }
  }

  async getDevices() {
    if (this.deviceList) return this.deviceList;

    const response = await fetch('http://localhost:9881/devices');
    this.deviceList = await response.json();

    return this.deviceList;
  }

  async setSelectedDevice(options: SelectDeviceDTO) {
    this.selectedDevice = options.device;
    return fetch('http://localhost:9881/devices', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json', // Specify the content type of the body
      },
      body: JSON.stringify(options),
    });
  }
}
