import WebScanner, { type BitDepth, type PaperSource, type SelectDeviceDTO } from './scanner.ts';

const devicesButton = document.getElementById('devices-button');
const scanButton = document.getElementById('scan-button');
const saveButton = document.getElementById('save-button');
const selectedDevice: HTMLSelectElement = document.getElementById('selected-device') as HTMLSelectElement;
const dpi: HTMLSelectElement = document.getElementById('dpi') as HTMLSelectElement;
const paperSource: HTMLSelectElement = document.getElementById('paperSource') as HTMLSelectElement;
const bitDepth: HTMLSelectElement = document.getElementById('bitDepth') as HTMLSelectElement;
const scanner = new WebScanner();

devicesButton
  .addEventListener('click', async () => {
    console.log('getting devices');
    const devices = await scanner.getDevices();
    console.log(devices);

    const options = ['<option>-- Select a Scanner --</option>'];
    devices.forEach((d) => {
      options.push(`<option value="${d.ID}">${d.Name}</option>`);
    });

    selectedDevice.innerHTML = options.join('');
  });
selectedDevice.addEventListener('change', async () => {

});
saveButton
  .addEventListener('click', async () => {
    console.log('starting scan server');
    const { value } = selectedDevice;
    console.log('value changed', value);
    const device = scanner.deviceList.find((d) => d.ID === value);
    const options: SelectDeviceDTO = {
      device,
      dpi: Number(dpi.value),
      paperSource: paperSource.value as unknown as PaperSource,
      bitDepth: bitDepth.value as unknown as BitDepth,
    };

    if (device) {
      console.log('saving device', options);
      await scanner.setSelectedDevice(options);
    }
    console.log('scanner started');
  });
scanButton
  .addEventListener('click', async () => {
    scanner.scan();
  });
